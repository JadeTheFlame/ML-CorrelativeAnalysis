using Microsoft.ML;
using Microsoft.ML.Data;
using Mvvm;
using Mvvm.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ViewModels
{
    internal struct CorrelationData
    {
        internal Dictionary<string, List<double>> data;
    }

    internal class HeatMapPageViewModel : ViewModelBase
    {
        private MLContext _mlContext = new MLContext(seed: null);

        public Task<CorrelationData> LoadCorrelationData()
        {
            return Task.Run(async () =>
            {
                try
                {
                    var trainingDataPath = await MlDotNet.FilePath(@"ms-appx:///Data/sampledata.csv");
                    var readerOptions = new TextLoader.Options()
                    {
                        Separators = new[] { ',' },
                        HasHeader = true,
                        AllowQuoting = true,
                        Columns = new[]
                        {
                            new TextLoader.Column("PassengerId", DataKind.Single, 0),
                            new TextLoader.Column("Survived", DataKind.Single, 1),
                            new TextLoader.Column("Pclass", DataKind.Single, 2),
                            new TextLoader.Column("Sex", DataKind.Single, 3),
                            new TextLoader.Column("Age", DataKind.Single, 4)
                        }
                    };

                    var dataView = _mlContext.Data.LoadFromTextFile(trainingDataPath, readerOptions);
                    var result = new CorrelationData()
                    {
                        data = new Dictionary<string, List<double>>()
                    };
                    for (int i = 0; i < dataView.Schema.Count; i++)
                    {
                        var column = dataView.Schema[i];
                        result.data.Add(column.Name, dataView.GetColumn<float>(column).Select(f =>
                        {
                            return (double)f;
                        }).ToList());
                    }

                    return result;
                }
                catch (System.Exception e)
                {
                    if (System.Diagnostics.Debugger.IsAttached) System.Diagnostics.Debugger.Break();
                    throw;
                }
            });
        }
    }
}

