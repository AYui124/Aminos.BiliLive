using Aminos.BiliLive.ViewModels;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using System;

namespace Aminos.BiliLive
{
    public class ViewLocator : IDataTemplate
    {
        public Control? Build(object? param)
        {
            if(param is not ViewModelBase viewBase)
            {
                return null;
            }
            var viewName = viewBase.FinalViewName;
            var type = Type.GetType(viewName);

            if (type != null)
            {
                var view = (Control)Activator.CreateInstance(type)!;
                view.DataContext = param;
                return view;
            }

            return new TextBlock { Text = "Not Found: " + viewName };
        }

        public bool Match(object? data)
        {
            return data is ViewModelBase;
        }
    }
}
