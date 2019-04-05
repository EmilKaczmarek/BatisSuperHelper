---
id: GoToQueryFromXml
title: Navigate from Xml to Code
sidebar_label: Go to query XML->Code
---
## Showcase
![gif-codeToXml](assets/XmlCode.gif)

## Description
Use "Go To Query" button, and visual studio will open, and highlight query.
This feature is very flexible with cursor placing. It means that it does not matter where your cursor is placed, it should always detect query.
For example in dynamic query:
```Xml
<select id="GetByParameters" resultClass="dto" parameterClass="parameterDto">
    SELECT 
        t.Col1,
        t.Col2,
        t.Col3,
        t.Col4,
    FROM TABLE t
    <dynamic prepend="WHERE">
        <isNotNull prepend="AND" property="Col1">
            t.Col1 = #Col1#
        </isNotNull>
    </dynamic> 
</select>
```
"go to query" should execute when curson is at any position.

## TODO
This feature seems to be complete at this moment.
