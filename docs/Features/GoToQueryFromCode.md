---
id: GoToQueryFromCode
title: Navigate from Code to XML
sidebar_label: Go to query Code->XML
---

## Showcase
![gif-codeToXml](assets/CodeXml.gif)

## Description
Use "Go To Query" button, and visual studio will open, and highlight query.
Default keyboard shortcut is CTRL+SHIFT+=. It is good idea to change it to CTRL+SHIFT+F12, when you are using F12 for "Go to definition" and CTRL+F12 for "Go to implementation", and you are not using "Go to next error".

## Supported syntaxes
### String literals
```C#
Mapper.Instance().QueryForObject<int>("SomeQuery", true);
```
### Add expression
```C#
Mapper.Instance().QueryForObject<int>("Some" + "Query", true);
```
### Identifiers
```C#
string myQuery = "SomeQuery";
Mapper.Instance().QueryForObject<int>(myQuery, true);
```
### Interpolation
```C#
string partOfMyQuery = "Some";
Mapper.Instance().QueryForObject<int>($"{partOfMyQuery}Some", true);
```
### Compile time constants
```C#
Mapper.Instance().QueryForObject<int>($"{nameof(GetMyDto)}Some", true);
```
### typeof().Name
```C#
Mapper.Instance().QueryForObject<int>($"{typeof(GetMyDto).Name}Some", true);
```
### string.Format
```C#
Mapper.Instance().QueryForObject<int>(string.Format("{0}", "myQuery"), true);
```
### Fields
```C#
public string MyQuery = "MyQuery";

public void ExampleCall()
{
    Mapper.Instance().QueryForObject<int>(MyQuery, true);
}
```
### Properties
```C#
public string MyQuery {
    get
    {
        return "MyQuery";
    }
    set { } 
}

public void ExampleCall()
{
    Mapper.Instance().QueryForObject<int>(MyQuery, true);
}
```
### Lambda properties
```C#
public string MyQuery => "MyQuery";

public void ExampleCall()
{
    Mapper.Instance().QueryForObject<int>(MyQuery, true);
}
```
### Combinations of above syntaxes
```C#
public class Query { }
string QueryPart = "M";
string MyQuery => string.Format("{0}{1}", $"{QueryPart}y", typeof(Query).Name);
public void ExampleCall()
{
    Mapper.Instance().QueryForObject<int>(MyQuery, true);
}
```
## TODO
### Support more cases if needed
### Handle other code formatting cases
Right now, code formatted like this:
```C#
Mapper
    .Instance()
        .QueryForObject
        <int>
        (MyQuery, 
        true);

```
Can have issues with detecting query.