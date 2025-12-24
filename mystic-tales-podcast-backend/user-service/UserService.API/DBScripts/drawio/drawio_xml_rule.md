# DrawIO Database Diagram XML Generation Rules

## 📋 OVERVIEW
This document defines the complete rules for generating DrawIO XML files representing database schemas for the **Mystic Tales Podcast** project.

---

## 🎯 CORE PRINCIPLES

### 1. **XML Structure Foundation**
```xml
<?xml version="1.0" encoding="UTF-8"?>
<mxfile host="app.diagrams.net" agent="Mozilla/5.0" version="29.2.4">
  <diagram id="MbEjH1huSBzFkDJzeNuB" name="MTP_UserDb_dev">
    <mxGraphModel dx="1422" dy="794" grid="1" gridSize="10" guides="1" tooltips="1" connect="1" arrows="1" fold="1" page="1" pageScale="1" pageWidth="850" pageHeight="1100" math="0" shadow="0">
      <root>
        <mxCell id="0" />
        <mxCell id="1" parent="0" />
        <!-- All diagram elements go here -->
      </root>
    </mxGraphModel>
  </diagram>
</mxfile>
```

---

## 🏗️ TABLE DEFINITION RULES

### Rule 1.1: Table Container
```xml
<mxCell id="{tablename}-table" 
        value="{TableName}" 
        style="swimlane;fontStyle=1;childLayout=stackLayout;horizontal=1;startSize=30;horizontalStack=0;resizeParent=1;resizeParentMax=0;resizeLast=0;collapsible=0;marginBottom=0;swimlaneFillColor=#dae8fc;fillColor=#dae8fc;" 
        vertex="1" 
        parent="1">
  <mxGeometry x="{x}" y="{y}" width="{width}" height="{height}" as="geometry" />
</mxCell>
```

**Naming Convention:**
- `id`: `{tablename}-table` (lowercase, hyphenated)
- `value`: `{TableName}` (PascalCase)

### Rule 1.2: Column/Field Definition
```xml
<!-- Primary Key -->
<mxCell id="{tablename}-{fieldname}" 
        value="{fieldName} ( PK ): {DATATYPE}" 
        style="text;strokeColor=none;fillColor=none;align=left;verticalAlign=middle;spacingLeft=10;spacingRight=4;overflow=hidden;rotatable=0;points=[[0,0.5],[1,0.5]];portConstraint=eastwest;fontStyle=5;" 
        vertex="1" 
        parent="{tablename}-table">
  <mxGeometry y="{offsetY}" width="{width}" height="30" as="geometry" />
</mxCell>

<!-- Foreign Key -->
<mxCell id="{tablename}-{fieldname}" 
        value="{fieldName} ( FK ): {DATATYPE}" 
        style="text;strokeColor=none;fillColor=none;align=left;verticalAlign=middle;spacingLeft=10;spacingRight=4;overflow=hidden;rotatable=0;points=[[0,0.5],[1,0.5]];portConstraint=eastwest;fontStyle=2;" 
        vertex="1" 
        parent="{tablename}-table">
  <mxGeometry y="{offsetY}" width="{width}" height="30" as="geometry" />
</mxCell>

<!-- Regular Column -->
<mxCell id="{tablename}-{fieldname}" 
        value="{fieldName}: {DATATYPE}" 
        style="text;strokeColor=none;fillColor=none;align=left;verticalAlign=middle;spacingLeft=10;spacingRight=4;overflow=hidden;rotatable=0;points=[[0,0.5],[1,0.5]];portConstraint=eastwest;" 
        vertex="1" 
        parent="{tablename}-table">
  <mxGeometry y="{offsetY}" width="{width}" height="30" as="geometry" />
</mxCell>
```

**Field Styles:**
- **PK (Primary Key)**: `fontStyle=5` (bold + italic)
- **FK (Foreign Key)**: `fontStyle=2` (italic)
- **Regular**: `fontStyle=0` (normal)

**Height Calculation:**
```
tableHeight = 30 (header) + (numberOfFields × 30)
```

**Y-offset for fields:**
```
First field:  y = 30
Second field: y = 60
Third field:  y = 90
...and so on (increment by 30)
```

---

## 🔗 RELATIONSHIP RULES

### ⚠️ **CRITICAL RULE: NO OVERLAPPING CONNECTION POINTS**

> **Each relationship line MUST have a UNIQUE connection point.**
> **NEVER allow multiple lines to touch the same cell at the same position.**

### Rule 2.1: Relationship Cardinality Mapping

| Relationship Type | startArrow | endArrow | Description |
|-------------------|------------|----------|-------------|
| **N:1** (Many-to-One) | `ERmany` | `ERone` | Standard FK relationship |
| **1:1** (One-to-One) | `ERone` | `ERone` | Unique constraint on FK |
| **N:M** (Many-to-Many) | `ERmany` | `ERmany` | Junction table |

### Rule 2.2: Connection Point Strategy

When **multiple FKs from different tables** point to the **same target table**:

```xml
<!-- ❌ WRONG: All pointing to the same cell -->
<mxCell id="rel-1" ... target="account-id" />
<mxCell id="rel-2" ... target="account-id" />
<mxCell id="rel-3" ... target="account-id" />

<!-- ✅ CORRECT: Each points to a different cell -->
<mxCell id="rel-1" ... target="account-id" />
<mxCell id="rel-2" ... target="account-email" />
<mxCell id="rel-3" ... target="account-password" />
```

### Rule 2.3: Self-Referencing Tables

When a table has **2+ FKs pointing to the SAME table** (e.g., `AccountFollowedPodcaster`):

```xml
<!-- Table with 2 FKs to Account -->
<mxCell id="followpod-accountId" value="accountId ( PK-FK ): INT" ... />
<mxCell id="followpod-podcasterId" value="podcasterId ( PK-FK ): INT" ... />

<!-- ✅ CORRECT: Different connection points -->
<mxCell id="rel-followpod-account" 
        source="followpod-accountId" 
        target="account-fullName">  <!-- Different cell! -->
</mxCell>

<mxCell id="rel-followpod-podcaster" 
        source="followpod-podcasterId" 
        target="account-phone">  <!-- Different cell! -->
</mxCell>
```

### Rule 2.4: Standard Relationship Template

```xml
<mxCell id="rel-{source}-{target}" 
        style="edgeStyle=orthogonalEdgeStyle;rounded=0;orthogonalLoop=1;jettySize=auto;html=1;endArrow={arrowType};endFill=0;startArrow={arrowType};startFill=0;exitX={exitX};exitY={exitY};exitDx=0;exitDy=0;entryX={entryX};entryY={entryY};entryDx=0;entryDy=0;" 
        edge="1" 
        parent="1" 
        source="{sourceCell}" 
        target="{targetCell}">
  <mxGeometry relative="1" as="geometry">
    <Array as="points">
      <mxPoint x="{x1}" y="{y1}" />
      <mxPoint x="{x2}" y="{y2}" />
    </Array>
  </mxGeometry>
</mxCell>
```

**Exit/Entry Coordinates:**
- `0` = Left/Top
- `0.5` = Center
- `1` = Right/Bottom

**Routing with Waypoints:**
Use `<Array as="points">` to avoid overlapping lines:

```xml
<Array as="points">
  <mxPoint x="380" y="995" />  <!-- Intermediate point 1 -->
  <mxPoint x="380" y="175" />  <!-- Intermediate point 2 -->
</Array>
```

---

## 📐 LAYOUT GUIDELINES

### Rule 3.1: Table Positioning Strategy

```
┌─────────────┐         ┌──────────────┐         ┌─────────────┐
│   Lookup    │         │    Master    │         │   Related   │
│   Tables    │────────▶│    Table     │◀────────│   Tables    │
│  (x=40)     │         │   (x=450)    │         │  (x=900)    │
└─────────────┘         └──────────────┘         └─────────────┘
```

**Standard X-coordinates:**
- **Left zone** (Lookup/Reference): `x = 40`
- **Center zone** (Main/Master): `x = 450`
- **Right zone** (Child/Detail): `x = 900`

**Vertical spacing:**
- First table: `y = 100`
- Subsequent tables: `y = previous.y + previous.height + 60`

### Rule 3.2: Title Block

```xml
<mxCell id="db-title-1" 
        value="{DATABASE NAME}" 
        style="text;html=1;strokeColor=none;fillColor=#dae8fc;align=center;verticalAlign=middle;whiteSpace=wrap;rounded=0;fontStyle=1;fontSize=14;" 
        vertex="1" 
        parent="1">
  <mxGeometry x="40" y="40" width="400" height="30" as="geometry" />
</mxCell>
```

---

## ✅ VALIDATION CHECKLIST

Before finalizing the XML, verify:

- [ ] All table IDs follow `{tablename}-table` format
- [ ] All field IDs follow `{tablename}-{fieldname}` format
- [ ] PK fields have `fontStyle=5`
- [ ] FK fields have `fontStyle=2`
- [ ] **NO two relationships point to the same target cell**
- [ ] Self-referencing tables have distinct connection points
- [ ] All relationships have correct cardinality arrows
- [ ] Waypoints are used to avoid line overlap
- [ ] Table heights match field count: `30 + (fields × 30)`
- [ ] All geometry values are valid integers

---

## 🚫 COMMON MISTAKES TO AVOID

### ❌ Mistake 1: Overlapping Connection Points
```xml
<!-- WRONG -->
<mxCell id="rel-1" target="account-id" />
<mxCell id="rel-2" target="account-id" />  <!-- ❌ Same target! -->
```

### ❌ Mistake 2: Missing Waypoints for Self-References
```xml
<!-- WRONG: Direct connection will overlap -->
<mxCell id="rel-followpod-account" 
        source="followpod-accountId" 
        target="account-id">
  <!-- ❌ No waypoints! -->
</mxCell>
```

### ❌ Mistake 3: Incorrect Field Height Calculation
```xml
<!-- WRONG: Table has 5 fields but height is only 120 -->
<mxGeometry x="40" y="100" width="250" height="120" as="geometry" />
<!-- CORRECT should be: 30 + (5 × 30) = 180 -->
```

---

## 📚 DATA TYPE MAPPING

| C# Type | SQL Server Type | DrawIO Display |
|---------|----------------|----------------|
| `int` | `INT` | `INT` |
| `string` | `NVARCHAR(n)` | `NVARCHAR(n)` |
| `string` (unlimited) | `NVARCHAR(MAX)` | `NVARCHAR(MAX)` |
| `DateTime` | `DATETIME` | `DATETIME` |
| `decimal` | `DECIMAL(p,s)` | `DECIMAL(p,s)` |
| `bool` | `BIT` | `BIT` |
| `Guid` | `UNIQUEIDENTIFIER` | `UNIQUEIDENTIFIER` |
| `float` | `FLOAT` | `FLOAT` |
| `byte[]` | `VARBINARY(MAX)` | `VARBINARY(MAX)` |

---

## 🎨 COLOR SCHEME

```xml
<!-- Table header and cells -->
fillColor="#dae8fc"
swimlaneFillColor="#dae8fc"

<!-- Text -->
strokeColor="none"
fillColor="none"
```

---

## 📝 EXAMPLE: Complete Table Definition

```xml
<!-- Account Table Example -->
<mxCell id="account-table" value="Account" style="swimlane;fontStyle=1;childLayout=stackLayout;horizontal=1;startSize=30;horizontalStack=0;resizeParent=1;resizeParentMax=0;resizeLast=0;collapsible=0;marginBottom=0;swimlaneFillColor=#dae8fc;fillColor=#dae8fc;" vertex="1" parent="1">
  <mxGeometry x="450" y="100" width="350" height="150" as="geometry" />
</mxCell>

<!-- PK Field -->
<mxCell id="account-id" value="id ( PK ): INT" style="text;strokeColor=none;fillColor=none;align=left;verticalAlign=middle;spacingLeft=10;spacingRight=4;overflow=hidden;rotatable=0;points=[[0,0.5],[1,0.5]];portConstraint=eastwest;fontStyle=5;" vertex="1" parent="account-table">
  <mxGeometry y="30" width="350" height="30" as="geometry" />
</mxCell>

<!-- Regular Field -->
<mxCell id="account-email" value="email: VARCHAR(250)" style="text;strokeColor=none;fillColor=none;align=left;verticalAlign=middle;spacingLeft=10;spacingRight=4;overflow=hidden;rotatable=0;points=[[0,0.5],[1,0.5]];portConstraint=eastwest;" vertex="1" parent="account-table">
  <mxGeometry y="60" width="350" height="30" as="geometry" />
</mxCell>

<!-- FK Field -->
<mxCell id="account-roleId" value="roleId ( FK ): INT" style="text;strokeColor=none;fillColor=none;align=left;verticalAlign=middle;spacingLeft=10;spacingRight=4;overflow=hidden;rotatable=0;points=[[0,0.5],[1,0.5]];portConstraint=eastwest;fontStyle=2;" vertex="1" parent="account-table">
  <mxGeometry y="90" width="350" height="30" as="geometry" />
</mxCell>

<!-- Regular Field -->
<mxCell id="account-createdAt" value="createdAt: DATETIME" style="text;strokeColor=none;fillColor=none;align=left;verticalAlign=middle;spacingLeft=10;spacingRight=4;overflow=hidden;rotatable=0;points=[[0,0.5],[1,0.5]];portConstraint=eastwest;" vertex="1" parent="account-table">
  <mxGeometry y="120" width="350" height="30" as="geometry" />
</mxCell>

<!-- Relationship: N:1 to Role -->
<mxCell id="rel-account-role" style="edgeStyle=orthogonalEdgeStyle;rounded=0;orthogonalLoop=1;jettySize=auto;html=1;endArrow=ERone;endFill=0;startArrow=ERmany;startFill=0;exitX=0;exitY=0.5;exitDx=0;exitDy=0;entryX=1;entryY=0.5;entryDx=0;entryDy=0;" edge="1" parent="1" source="account-roleId" target="role-id">
  <mxGeometry relative="1" as="geometry" />
</mxCell>
```

---

## 🔄 VERSION CONTROL

**Document Version:** 1.0.0  
**Last Updated:** December 8, 2025  
**Author:** GitHub Copilot (Claude Sonnet 4.5)  
**Project:** Mystic Tales Podcast - User Service Database

---

## 📌 QUICK REFERENCE

### Arrow Types
- `ERone` = Single line (1)
- `ERmany` = Crow's foot (N)
- `endFill=0` = Hollow arrowhead
- `startFill=0` = Hollow start

### Font Styles
- `0` = Normal
- `1` = Bold
- `2` = Italic
- `4` = Underline
- `5` = Bold + Italic

### Exit/Entry Points
```
      entryX=0.5, entryY=0 (top)
               ▲
               │
entryX=0 ◄─────┼─────► entryX=1
entryY=0.5     │       entryY=0.5
(left)         │       (right)
               ▼
      entryX=0.5, entryY=1 (bottom)
```

---

## ✨ FINAL NOTES

1. **Always validate** the XML in DrawIO after generation
2. **Test all relationships** by hovering over connections
3. **Verify no overlaps** by zooming in on connection points
4. **Check table heights** match the actual rendered height
5. **Use consistent spacing** for professional appearance

**Remember:** DrawIO XML is sensitive to formatting. Follow these rules exactly for best results! 🎯