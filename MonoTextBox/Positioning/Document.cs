using System.Diagnostics;
using Icu;
using MonoTextBox.Positioning.Enumerating;
using MonoTextBox.Utils;

namespace MonoTextBox.Positioning;

// Changing locale and width is effectively the same as calculating a new Document,
// therefore not implemented
public class Document : Node<ParagraphBranch>
{
    private readonly float _width;

    private readonly Locale? _locale;


    private Document(
        ParagraphBranch branch,
        float width,
        Locale? locale)
        : base(branch, null, null)
    {
        _width = width;
        _locale = locale;
    }

    public static Document Build(
        float width,
        Buffer buffer,
        Locale? locale = null)
    {
        Document? doc = null;

        foreach (var offset in buffer.EnumerateNewLines())
        {
            var paragraph = ParagraphBranch.Build(width, buffer.Slice(offset), locale);

            if (doc is null)
                doc = new Document(paragraph, width, locale);
            else
                doc = (Document)doc.AppendAndBalance(paragraph);
        }

        Debug.Assert(doc is not null);
        return doc;
    }


    public Document Insert(int start, int length, Buffer insertedDoc)
    {
        var inserted = insertedDoc.Slice(start, length);
        return !HasNewLine(inserted)
            ? InsertInLine(start, length, insertedDoc)
            : InsertMultiLines(start, length, insertedDoc);
    }

    private Document InsertInLine(int start, int length, Buffer doc)
    {
        var (paragraph, index) = Find(start);
        var paraStart = start - index;
        var paraLength = length + paragraph.Length;
        var updatedParagraph = doc.Slice(paraStart, paraLength);

        paragraph.Update(_width, updatedParagraph, index, _locale);

        return this;
    }

    private Document InsertMultiLines(int start, int length, Buffer doc)
    {
        var result = this;
        var lastSlice = new Slice(-1, -1); // Delay append to insert last new line to right

        foreach (var slice in doc.Slice(start, length).EnumerateNewLines())
        {
            var absolute = slice + start;
            if (lastSlice.Start == -1)
            {
                lastSlice = absolute;
                continue;
            }

            result = result.InsertSplitLine(lastSlice, doc, true);
            lastSlice = slice;
        }

        result = result.InsertSplitLine(lastSlice, doc, false);

        return result;
    }

    private Document InsertSplitLine(Slice slice, Buffer doc, bool toLeft)
    {
        var (result, left, right) = FindAndSplit(slice.Start);

        var target = toLeft ? left : right;
        var start = toLeft ? slice.Start - target.Length : slice.Start;
        var length = target.Length + slice.Length;
        var content = doc.Slice(start, length);
        target.Update(_width, content, slice.Start, _locale);

        return result;
    }


    public Document Remove(
        int start,
        ReadOnlySpan<char> removed,
        Buffer removedDoc)
    {
        return !HasNewLine(removed)
            ? RemoveInLine(start, removed, removedDoc)
            : RemoveMultiLines(start, removed, removedDoc);
    }

    private Document RemoveInLine(int start, ReadOnlySpan<char> removed, Buffer doc)
    {
        var absoluteIndex = start;
        var (paragraph, relativeIndex) = Find(absoluteIndex);
        RemoveParagraphLine(doc, absoluteIndex, paragraph, relativeIndex, removed.Length);
        return this;
    }

    private Document RemoveMultiLines(int start, ReadOnlySpan<char> removed, Buffer doc)
    {
        var result = this;

        (result, var first, var last) = result.RemoveLinesAndPop(start, removed, doc);

        var firstStart = start - first.Length;
        var merged = MergeParagraph(doc, firstStart, first, last);
        result = (Document)result.InsertAndBalance(firstStart, merged);
        
        return result;
    }

    private (Document Doc, ParagraphBranch First, ParagraphBranch Last) RemoveLinesAndPop(
        int start,
        ReadOnlySpan<char> removed,
        Buffer doc)
    {
        Document? document = this;
        ParagraphBranch? first = null;
        ParagraphBranch? last = null;
        foreach (var slice in removed.EnumerateNewLines())
        {
            var absoluteStart = start;
            var length = slice.Length;

            var (paragraph, relativeStart) = Find(absoluteStart);

            if (length > 0)
                RemoveParagraphLine(doc, absoluteStart, paragraph, relativeStart, length);
            if (first is null)
                first = paragraph;
            else
                last = paragraph;
            document = (Document?)RemoveAndBalance(absoluteStart);
        }

        Debug.Assert(first is not null && last is not null && document is not null);
        return (document, first, last);
    }

    private void RemoveParagraphLine(
        Buffer doc,
        int absoluteIndex,
        ParagraphBranch paragraph,
        int relativeIndex,
        int removedLength)
    {
        Debug.Assert(removedLength > 0);

        var start = absoluteIndex - relativeIndex;
        var length = paragraph.Length - removedLength;
        var updated = doc.Slice(start, length);
        paragraph.Update(_width, updated, relativeIndex, _locale);
    }



    private (Document Doc, ParagraphBranch Left, ParagraphBranch Right) FindAndSplit(int index)
    {
        var (branch, innerIndex) = Find(index);
        var branchStart = index - innerIndex;
        var branchEnd = branchStart + branch.Length;

        Node<ParagraphBranch> doc;
        ParagraphBranch left;
        ParagraphBranch right;
        if (innerIndex == 0)
        {
            left = ParagraphBranch.Empty(_width, _locale);
            right = branch;
            doc = InsertAndBalance(branchStart, left);
        }
        else if (innerIndex == branch.Length)
        {
            left = branch;
            right = ParagraphBranch.Empty(_width, _locale);
            doc = InsertAndBalance(branchEnd, right);
        }
        else
        {
            (left, right) = branch.Split(innerIndex);
            var removed = RemoveAndBalance(branchStart);
            var inserted = removed?.InsertAndBalance(branchStart, right) ?? new Document(right, _width, _locale);
            doc = inserted.InsertAndBalance(branchStart, left);
        }

        return ((Document)doc, left, right);
    }


    private ParagraphBranch MergeParagraph(
        Buffer doc,
        int start,
        ParagraphBranch left, 
        ParagraphBranch right)
    {
        var length = left.Length + right.Length;
        var content = doc.Slice(start, length);
        return ParagraphBranch.Build(_width, content, _locale);
    }


    private static bool HasNewLine(Buffer buffer) => HasNewLine(buffer.Text);
    
    private static bool HasNewLine(ReadOnlySpan<char> text) => text.IndexNewLine() >= 0;
}


public class ParagraphBranch: IBranch<ParagraphBranch>
{
    private readonly Paragraph _paragraph;
    
    
    public int Length => _paragraph.CharCount;
    public int LineCount => _paragraph.LineCount;


    private ParagraphBranch(Paragraph paragraph)
    {
        _paragraph = paragraph;
    }

    private ParagraphBranch(List<Line> lines): this(new Paragraph(lines))
    { }
    
    
    public static ParagraphBranch Empty(float lineWidth, Locale? locale)
        => Build(lineWidth, Buffer.Empty, locale);
    
    public static ParagraphBranch Build(
        float lineWidth,
        Buffer paragraph,
        Locale? locale)
    {
        return new ParagraphBranch(
            Paragraph.Build(lineWidth, paragraph, locale)
        );
    }


    public void Update(
        float lineWidth,
        Buffer paragraph,
        int changeIndex,
        Locale? locale)
    {
        _paragraph.Update(lineWidth, paragraph, changeIndex, locale);
    }

    
    public (ParagraphBranch, ParagraphBranch) Split(int index)
    {
        Debug.Assert(0 < index && index < Length);
        
        var lineIndex = FindLine(index);
        var leftPart = _paragraph.Lines[lineIndex].Positions.Take(index);
        var rightPart = _paragraph.Lines[lineIndex].Positions.Skip(index);
        
        var rightCount = _paragraph.Lines.Count - lineIndex;
        var rightLine = new Line(rightPart);
        var rightLines = new List<Line>(rightCount){rightLine};
        rightLines.AddRange(_paragraph.Lines.Skip(lineIndex));
        
        var leftLines = _paragraph.Lines.Take(lineIndex).ToList();
        var leftLine = new Line(leftPart);
        leftLines.Add(leftLine);

        return (
            new ParagraphBranch(leftLines), 
            new ParagraphBranch(rightLines)
        );
    }


    private int FindLine(int charIndex)
    {
        var sum = 0;
        for (var i = 0; i < _paragraph.Lines.Count; i++)
        {
            sum += _paragraph.Lines[i].Length;
            if (sum <= charIndex)
                continue;
            return i;
        }
        throw new IndexOutOfRangeException();
    }
}