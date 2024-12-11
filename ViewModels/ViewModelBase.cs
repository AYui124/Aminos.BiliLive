using CommunityToolkit.Mvvm.ComponentModel;
using Material.Icons;
using SukiUI.Content;

namespace Aminos.BiliLive.ViewModels
{
    public abstract class ViewModelBase : ObservableObject
    {
        private const string ViewBaseNamespace = "Aminos.BiliLive.Views";

        public abstract MaterialIconKind Icon { get; }

        public abstract string ModelName { get; }

        protected abstract string ViewName {  get; }

        public abstract string MenuName {  get; }

        public abstract int Index  { get; }
        public string FinalViewName => $"{ViewBaseNamespace}.{ViewName}";


    }
}
