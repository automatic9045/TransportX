using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using TransportX.Collections;
using TransportX.Diagnostics;
using TransportX.Rendering;

namespace TransportX.Scripting.Collections
{
    internal class ScriptModelCollection : IModelCollection
    {
        private readonly IErrorCollector ErrorCollector;

        private readonly ScriptDictionary<string, IModel> Models;
        private readonly ScriptKeyedList<string, IModelBundle> BundlesKey;

        public IReadOnlyKeyedList<string, IModelBundle> Bundles => BundlesKey;

        public ScriptModelCollection(IErrorCollector errorCollector)
        {
            ErrorCollector = errorCollector;

            Models = new ScriptDictionary<string, IModel>(errorCollector, "モデル", key => Model.Empty());
            BundlesKey = new ScriptKeyedList<string, IModelBundle>(bundle => bundle.Key, errorCollector, "モデルバンドル", ModelBundle.Empty);
        }

        public void Dispose()
        {
            foreach (IModelBundle bundle in Bundles)
            {
                bundle.Dispose();
            }
        }

        public IModel GetModel(string modelKey)
        {
            return modelKey == string.Empty ? Model.Empty() : Models[modelKey];
        }

        public bool TryGetModel(string modelKey, [MaybeNullWhen(false)] out IModel model)
        {
            if (modelKey == string.Empty)
            {
                model = Model.Empty();
                return true;
            }
            else
            {
                return Models.TryGetValue(modelKey, out model);
            }
        }

        public bool AdoptBundle(IModelBundle bundle, bool allowOverride = false)
        {
            if (Bundles.TryGetValue(bundle.Key, out IModelBundle? existingBundle))
            {
                if (existingBundle == bundle)
                {
                    ScriptError error = new(ErrorLevel.Error, "指定したモデルバンドルは既に登録されています。");
                    ErrorCollector.Report(error);
                    return false;
                }

                if (allowOverride)
                {
                    ReleaseBundle(bundle.Key);
                }
                else
                {
                    ScriptError error = new(ErrorLevel.Error, $"キー '{bundle.Key}' のモデルバンドルは既に存在します。");
                    ErrorCollector.Report(error);
                    return false;
                }
            }

            BundlesKey.Add(bundle);

            foreach ((string modelKey, IModel model) in bundle.Models)
            {
                Models.Add(modelKey, model);
            }

            return true;
        }

        public bool ReleaseBundle(string bundleKey)
        {
            if (!BundlesKey.GetValue(bundleKey, out IModelBundle? bundle)) return false;

            foreach (string modelKey in bundle.Models.Keys)
            {
                Models.Remove(modelKey);
            }

            BundlesKey.Remove(bundleKey);
            bundle.Dispose();

            return true;
        }
    }
}
