using System.ComponentModel;

namespace FrinkyEngine.Core.CanvasUI.Authoring;

/// <summary>
/// Optional marker interface for CanvasUI binding contexts.
/// Implementors should raise <see cref="INotifyPropertyChanged.PropertyChanged"/>
/// so one-way bindings can update.
/// </summary>
public interface ICanvasBindingContext : INotifyPropertyChanged
{
}
