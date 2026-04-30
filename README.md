# HeadlessTextBox
A headless text box engine written in C#. It handles text storage, editing, positioning, rich text metadata, and undo/redo history, but **not** rendering. It expects a custom renderer on top, and is intended for projects such as game UI, terminal-like interfaces, custom editors, or other non-standard UI systems.

## Status
> This project has not been fully tested or debugged yet. Expect bugs, missing edge cases, and API changes.


## Features
- Text storage based on a piece table and weighted tree, allowing efficient indexing and editing
- Position cache based on paragraphs as the invalidation unit, reducing repositioning work after edits
- Rich text styling metadata
- Word wrapping and line breaking based on the system ICU implementation
- Glyph positioning with support for:
  - Common font types such as `.ttf` and `.otf`, using HarfBuzz
  - Non-standard font types such as bitmap fonts, using manually provided metrics
- Rich text formatting support in glyph positioning
- Undo and redo support
- Storage tree compaction without destroying undo/redo history
- Serialization for text and rich text formatting
- Low-allocation implementation


## Why?
This project started as an attempt to separate text storage and positioning from rendering, since many available solutions are coupled to rendering backends or UI frameworks, or only address one part of the problem.
HeadlessTextBox is not intended to replace full text layout and rendering frameworks. It is meant to provide the editable text model underneath a custom renderer.


## Requirements
- C#
- .NET 6.0 or newer


## Limitations
These features are not currently supported, and may or may not be implemented in the future:
- Font fallback
- Inline images or embedded objects
- Non-rectangular text boxes
- Right-to-left text
- Syntax highlighting
- Spell checking
- Accessibility integration


## Not Implemented
HeadlessTextBox does not provide rendering or application-level UI behavior, including:
- Text rendering
- Caret rendering
- Selection rendering
- Clipboard integration
- Input handling
- Scrolling or viewport management
