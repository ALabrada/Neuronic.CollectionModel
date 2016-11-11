## Synopsis

Utility .Net library for declarative collection handling. It provides a set of query methods, resembling the ones in LINQ, which operate on read-only observable lists and collections, instead of [IEnumerable](https://msdn.microsoft.com/en-us/library/9eekhta0(v=vs.110).aspx). A read-only observable list is a generic list (Of T) that implements [IReadOnlyList\<T\>](https://msdn.microsoft.com/en-us/library/hh192385(v=vs.110).aspx), [INotifyCollectionChanged](https://msdn.microsoft.com/en-us/library/system.collections.specialized.inotifycollectionchanged(v=vs.110).aspx) and [INotifyPropertyChanged](https://msdn.microsoft.com/en-us/library/system.componentmodel.inotifypropertychanged(v=vs.110).aspx). Thus, it provides a powerful set of features:
- Genericity
- Covariance
- O(1) count
- Change notification.

It is particularly useful in application using Windows Presentation Foundation (WPF) and similar technologies, but can be used in other .Net applications as well. 

## Code Example

```
using Neuronic.CollectionModel;
...
var parents = new ObservableCollection<Person>( ... );
IReadOnlyList<Person> youngChildren = 
    parents.ListSelectMany(parent => parent.Children).ListWhere(child => child.Age < 5);
parents.Add(new Person( ... ));
```

## Installation

The binaries are available at [Nuget](https://www.nuget.org/packages/Neuronic.CollectionModel/). To install in run the command `Install-Package Neuronic.CollectionModel`.

## API Reference

The library provides a set of extension methods analogous to some of the [LINQ extension methods](https://msdn.microsoft.com/en-us/library/system.linq.enumerable(v=vs.110).aspx). For instance, there are two replacements for the [Cast](https://msdn.microsoft.com/en-us/library/bb341406(v=vs.110).aspx) extension method:
- `CollectionCast`, which operates on read-only observable collections (`IReadOnlyObservableCollection<T>`)
- `ListCast`, which operates on read-only observable lists (`IReadOnlyObservableList<T>`)

Some of the LINQ extension methods have only the List\* replacement because it can operate both on lists and collections and there is no performance benefit in implementing a collection-only variation. Other extension methods have one or both of its replacements missing just because they are not implemented jet. If you can implement your own, you are welcome to create a pull request.

The lists and collections generated by the extension methods listen for changes in the source sequences if they implement `INotifyCollectionChanged` and update the result accordingly. Also, some of them can listen for changes in the sequence items if they implement `INotifyPropertyChanged`. Thus, the previous code example can be modified to update the resulting children list if the age of a child changes:

```
IReadOnlyList<Person> youngChildren = 
    parents.ListSelectMany(parent => parent.Children).ListWhere(child => child.Age < 5, nameof(Person.Age));
```

## License

This project is licensed under the GNU Lesser General Public License (LGPL). As stated in [its Wikipedia article](https://en.wikipedia.org/wiki/GNU_Lesser_General_Public_License):
>The GNU Lesser General Public License (LGPL) is a free software license published by the Free Software Foundation (FSF). The license allows developers and companies to use and integrate software released under the LGPL into their own (even proprietary) software without being required by the terms of a strong copyleft license to release the source code of their own components. The license only requires software under the LGPL be modifiable by end users via source code availability. For proprietary software, code under the LGPL is usually used in the form of a shared library such as a DLL, so that there is a clear separation between the proprietary and LGPL components.

That is, you can compile and reference this library (as is) without restrictions. Otherwise, if you need to make changes to its source code, you should make the changes public (or create a pull request).
