namespace MarketFeed
{
    using Common;
    using Common.Helpers;
    using Common.Providers;
    using Common.Writers;
    using Microsoft.Extensions.Configuration;
    using System.Threading.Tasks;
    using MarketFeedHandler = List<FeedHandler>;

    internal class ProviderHandler
    {
        public required string Name = string.Empty;
        public required IConfigurationSection ConfigSection { get; set; }
        public required AbstractHandler PriceHandler { get; set; }
    }

    internal class WriterHandler
    {
        public required string Name = string.Empty;
        public required IConfigurationSection ConfigSection { get; set; }
        public required AbstractWriter Writer { get; set; }
    }
    internal class FeedHandler
    {
        public required ProviderHandler ProviderHandler { get; set; }
        public required List<WriterHandler> WriterHandler { get; set; } = [];
    }

    internal class Program
    {
        static async Task<bool> Init(MarketFeedHandler marketFeedHandler)
        {
            var allInitResults = marketFeedHandler
                                .Select(async handler =>
                                {
                                    var priceHandler = handler.ProviderHandler.PriceHandler;
                                    var providerConfigSection = handler.ProviderHandler.ConfigSection;

                                    try
                                    {
                                        // Call the async Init method directly on the AbstractWriter object (writerKvp.Key).
                                        var init = await priceHandler.Init(providerConfigSection);
                                        if (!init)
                                            return init;
                                    }
                                    catch (Exception ex)
                                    {
                                        Console.WriteLine($"[Init Logic]: ERROR Init price provider handler [{handler.ProviderHandler.Name}]: {ex.Message}");
                                        return false;
                                    }

                                    var individualWriterInitTasks = handler.WriterHandler
                                    .Select(async writerHandler =>
                                    {
                                        // Access the AbstractWriter instance using .Key property
                                        AbstractWriter writer = writerHandler.Writer;
                                        // Access the IConfigurationSection using .Value property
                                        IConfigurationSection configSection = writerHandler.ConfigSection;

                                        try
                                        {
                                            // Call the async Init method directly on the IWriter object (writerKvp.Key).
                                            bool success = await writer.Init(configSection);
                                            return success;
                                        }
                                        catch (Exception ex)
                                        {
                                            Console.WriteLine($"[Init Logic]: ERROR Init writer [{writer}] for price provider handler [{priceHandler}]: {ex.Message}");
                                            return false; // Return false for this specific task
                                        }
                                    })
                                    .ToList(); // IMPORTANT: .ToList() starts the tasks.

                                    return await TaskHelper.IsTaskListCompletedSuccessfully(individualWriterInitTasks);
                                })
                                .ToList();

            return await TaskHelper.IsTaskListCompletedSuccessfully(allInitResults);
        }

        static async Task<bool> Start(MarketFeedHandler marketFeedHandler)
        {
            var allStartResults = marketFeedHandler
                                .Select(async handler =>
                                {
                                    var individualWriterStartTasks = handler.WriterHandler
                                    .Select(async writerHandler =>
                                    {
                                        // Access the AbstractWriter instance using .Key property
                                        AbstractWriter writer = writerHandler.Writer;
                                        // Access the IConfigurationSection using .Value property

                                        try
                                        {
                                            // Call the async Start method directly on the IWriter object (writerKvp.Key).
                                            bool success = await writer.Start();
                                            return success;
                                        }
                                        catch (Exception ex)
                                        {
                                            Console.WriteLine($"[Start Logic]: ERROR Start writer [{writer}]: {ex.Message}");
                                            return false; // Return false for this specific task
                                        }
                                    })
                                    .ToList(); // IMPORTANT: .ToList() starts the tasks.

                                    await TaskHelper.IsTaskListCompletedSuccessfully(individualWriterStartTasks);

                                    var priceHandler = handler.ProviderHandler.PriceHandler;
                                    try
                                    {
                                        // Call the async Init method directly on the AbstractWriter object (writerKvp.Key).
                                        return await priceHandler!.Start();
                                        //if (!started)
                                        //    return started;
                                    }
                                    catch (Exception ex)
                                    {
                                        Console.WriteLine($"[Start Logic]: ERROR Start price provider handler [{handler.ProviderHandler.Name}]: {ex.Message}");
                                        return false;
                                    }
                                })
                                .ToList();

            return await TaskHelper.IsTaskListCompletedSuccessfully(allStartResults);
        }

        static async Task<bool> Stop(MarketFeedHandler marketFeedHandler)
        {
            var allStopResults = marketFeedHandler
                .Select(async handler =>
                {
                    var priceHandler = handler.ProviderHandler.PriceHandler;

                    try
                    {
                        var stopped = await priceHandler.Stop();
                        if (!stopped)
                            return stopped;
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"[Stop Logic]: ERROR Stop price provider handler [{handler.ProviderHandler.Name}]: {ex.Message}");
                        return false;
                    }

                    var individualWriterStopTasks = handler.WriterHandler
                    .Select(async writerHandler =>
                    {
                        // Access the AbstractWriter instance using .Key property
                        AbstractWriter writer = writerHandler.Writer;

                        try
                        {
                            // Call the async Init method directly on the AbstractWriter object (writerKvp.Key).
                            bool success = await writer.Stop();
                            return success;
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"[Stop Logic]: ERROR Stop writer [{writer}] for price provider handler [{priceHandler}]: {ex.Message}");
                            return false; // Return false for this specific task
                        }
                    })
                    .ToList(); // IMPORTANT: .ToList() starts the tasks.

                    return await TaskHelper.IsTaskListCompletedSuccessfully(individualWriterStopTasks);
                })
                .ToList();

            return await TaskHelper.IsTaskListCompletedSuccessfully(allStopResults);
        }

        static async Task<bool> AttachWriters(MarketFeedHandler marketFeedHandler)
        {
            var attachAllWritersTask = marketFeedHandler
                                    .Select(async feedHandler =>
                                    {
                                        var allWriters = feedHandler.WriterHandler
                                                            .Select(async writer =>
                                                            {
                                                                return await Task.Run(() =>
                                                                    {
                                                                        feedHandler.ProviderHandler.PriceHandler.Attach(writer.Writer);
                                                                        return true;
                                                                    });
                                                            })
                                                            .ToList();
                                        return await TaskHelper.IsTaskListCompletedSuccessfully(allWriters);
                                    })
                                    .ToList();
            return await TaskHelper.IsTaskListCompletedSuccessfully(attachAllWritersTask);
        }

        static async Task<bool> Execute(string configFile)
        {
            try
            {
                var configRoot = new ConfigurationBuilder().AddJsonFile(configFile, false, true).Build();

                var handlers = await GetHandlers(configRoot);

                if (handlers != null || handlers!.Count > 0)
                {
                    return
                           await Init(handlers!) &&
                           await AttachWriters(handlers!) &&
                           await Start(handlers!) &&
                           await Stop(handlers!);
                }
                return true;
            }
            catch (AggregateException ex)
            {
                Console.WriteLine($"\nCaught AggregateException with {ex.InnerExceptions.Count} inner exceptions:");
                foreach (var innerEx in ex.InnerExceptions)
                {
                    Console.WriteLine($"  - {innerEx.GetType().Name}: {innerEx.Message}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Caught unexpected exception: {ex.Message}");
            }

            return false;
        }

        static async Task<MarketFeedHandler> GetHandlers(IConfigurationRoot configurationRoot)
        {
            MarketFeedHandler feedHandlers = [];

            var providersSection = configurationRoot.GetSection("Providers");
            var providerTasks = providersSection.GetChildren()
                                .Select(async configurationSection =>
                                {

                                    var (identifier, priceHandler) = ProviderManager.GetPriceHandlerForProvider(configurationSection);
                                    var providerHandler = new ProviderHandler
                                    {
                                        Name = identifier.ProviderType.ToString(),
                                        ConfigSection = configurationSection,
                                        PriceHandler = priceHandler
                                    };

                                    List<Task<WriterHandler>> writerHandlers = [];
                                    List<WriterHandler> a = [];
                                    var writersSection = configurationSection.GetSection("ListOfWriters");
                                    writersSection.GetChildren()
                                                        .Select(writerSection =>
                                                        {
                                                            writerHandlers.Add(Task.Run(() =>
                                                            {
                                                                var (writerType, writerHandler) = WriterManager.GetWriterHandler(writerSection);
                                                                return new WriterHandler
                                                                {
                                                                    ConfigSection = writerSection,
                                                                    Name = writerType.ToString(),
                                                                    Writer = writerHandler
                                                                };

                                                            }));
                                                            return true;
                                                        }).ToList();
                                    feedHandlers.Add(new FeedHandler { ProviderHandler = providerHandler, WriterHandler = (await Task.WhenAll(writerHandlers)).ToList() });
                                    return true;
                                })
                                .ToList();

            var _ = await TaskHelper.IsTaskListCompletedSuccessfully(providerTasks);
            return feedHandlers;
        }

        public static async Task<int> Main(string[] args)
        {
            if (args.Length != 1)
            {
                Console.Error.WriteLine("Usage : < program name> <json config file>");
                return -1;
            }

            try
            {
                if (await Execute(args[0]))
                    return 0;
                return -2;
            }
            catch (Exception ex)
            {
                await Console.Error.WriteLineAsync($"Exception : {ex.Message}");
            }

            return -3;
        }
    }
}
