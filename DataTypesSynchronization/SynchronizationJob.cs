using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

using Composite.Core.Threading;
using Composite.Data;

namespace CompositeC1Contrib.DataTypesSynchronization
{
    public class SynchronizationJob
    {
        private static readonly object UpdateLock = new object();

        private readonly Logger _log;

        private readonly CancellationToken _cancellationToken;

        public SynchronizationJob(Guid jobId, CancellationToken cancellationToken)
        {
            _cancellationToken = cancellationToken;

            _log = new Logger(jobId);
        }

        public void UpdateOne<T>()
        {
            var type = DataTypesSynchronizationConfiguration.Current.TypesToSynchronize[typeof(T)];

            UpdateTypes(new[] { type });
        }

        public void UpdateMultiple(IEnumerable<Type> types)
        {
            var typeDefs = types.Select(t => DataTypesSynchronizationConfiguration.Current.TypesToSynchronize[t]);

            UpdateTypes(typeDefs);
        }

        public void UpdateAll(SynchronizationFrequency frequency)
        {
            var types = DataTypesSynchronizationConfiguration.Current.TypesToSynchronize.Values;
            if (frequency != SynchronizationFrequency.All)
            {
                types = types.Where(t => t.Frequency == frequency).ToList();
            }

            UpdateTypes(types);
        }

        private void UpdateTypes(IEnumerable<SynchronizationTypeDefinition> types)
        {
            lock (UpdateLock)
            {
                _log.AppendToLog("Job started");

                using (ThreadDataManager.EnsureInitialize())
                {
                    using (var data = new DataConnection())
                    {
                        try
                        {
                            foreach (var t in types)
                            {
                                _cancellationToken.ThrowIfCancellationRequested();

                                try
                                {
                                    t.Method.Invoke(this, new[] { t.Mapper, data, t.PostProcessor });
                                }
                                catch (OperationCanceledException)
                                {
                                    throw;
                                }
                                catch (ThreadAbortException exc)
                                {
                                    throw new OperationCanceledException("Thread aborted", exc);
                                }
                                catch (Exception exc)
                                {
                                    if (exc.InnerException is OperationCanceledException)
                                    {
                                        throw exc.InnerException;
                                    }

                                    _log.LogException(t.Type, exc);
                                }
                            }
                        }
                        catch (OperationCanceledException exc)
                        {
                            _log.AppendToLog("Operation canceled with message '{0}'", exc.Message);

                            throw;
                        }
                        catch (Exception exc)
                        {
                            _log.AppendToLog("Error when updating data: {0}", exc);
                        }
                    }
                }

                _log.AppendToLog("Job finished");

                _log.Dispose();
            }
        }

        private void UpdateModelType<TModel, TData>(IMapper<TModel, TData> mapper, DataConnection data, IPostProcessor<TData> postProcessor) where TData : class, IData
        {
            _log.AppendToLog("Synchronizing {0}", typeof(TModel).Name);

            var modelType = typeof(TModel);
            var provider = DataTypesSynchronizationConfiguration.Current.DataProviders.FirstOrDefault(d => d.IsProviderFor(modelType));
            if (provider == null)
            {
                return;
            }

            var iList = new List<TData>();
            var list = ((IEnumerable<TModel>)provider.GetData(modelType)).ToList();

            _log.AppendToLog("{0} objects retrieved from Proxy", list.Count);

            foreach (var itm in list)
            {
                _cancellationToken.ThrowIfCancellationRequested();

                var iData = data.CreateNew<TData>();

                mapper.Map(itm, iData);

                iList.Add(iData);
            }

            _log.AppendToLog("Finished converting to C1 type", iList.Count);

            if (list.Count != iList.Count)
            {
                _log.AppendToLog("Mismatch in numbers of proxy data and converted C1 items, only {0} items converted. Aborting synchronization.", iList.Count);

                return;
            }

            var comparer = new DataUpdateComparer<TData>(iList, _log, _cancellationToken);
            var result = comparer.DataUpdate();

            if (postProcessor != null)
            {
                postProcessor.Process(result);
            }

            _log.AppendToLog("Finished synchronizing {0}", typeof(TModel).Name);
        }
    }
}
