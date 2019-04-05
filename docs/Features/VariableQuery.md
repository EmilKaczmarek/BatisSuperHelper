---
id: VariableQuery
title: Variable/Generic Query
sidebar_label: Variable/Generic Query
---

## Description
Sometimes you will want to invoke query id, that is not resolvable in compile time.
For example:
```C#
public class GenericReadDAL<T>
{
    private string _namespace = typeof(T).Name;
    public T GetAll(){
        Mapper.Instance().QueryForObject<int>($"{_namespace}.GetAll", true);
    }
}
```
Variable(or generic) support will let you navigate to all statments with id of "GetAll", ignoring variable part(namespace in this case).
### Go to query from generic method
If your query id has only one unresolvable part, and this part needs T name, than you will have ability to Go to query from your generic method signature.
![gif-genericGoToQuery](assets/GenericGoToQuery.gif)

