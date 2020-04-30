using System;
using System.Diagnostics;
using System.IO;
using System.Collections.Generic;

namespace Core
{
    public interface ITextWriter
    {
        void Write(string s);
        void WriteLine(string s);
    }

    public static class Html
    {
        public readonly static IHtmlTag html = new HtmlTag("HTML");
        public readonly static IHtmlTag Body = new HtmlTag("BODY");
        public readonly static IHtmlTag Head = new HtmlTag("HEAD");
        public readonly static IHtmlTag Header1 = new HtmlTag("H1");
        public readonly static IHtmlTag Header2 = new HtmlTag("H2");
        public readonly static IHtmlTag Header3 = new HtmlTag("H3");
        public readonly static IHtmlTag Header4 = new HtmlTag("H4");
        public readonly static IHtmlTag Header5 = new HtmlTag("H5");
        public readonly static IHtmlTag Header6 = new HtmlTag("H6");
        public readonly static IHtmlTag Title = new HtmlTag("TITLE");
        public readonly static IHtmlTag Font = new HtmlTag("FONT");
        public readonly static IHtmlTag Paragraph = new HtmlTag("P");
        public readonly static IHtmlTag Break = new HtmlBrTag();
    }

    public interface IHtmlTag
    {
        bool Begin(StreamWriter writer);
        bool Begin(StreamWriter writer, HtmlAttribute attr);
        bool Begin(StreamWriter writer, HtmlAttribute[] attrs);
        void End(StreamWriter writer);
    }

    public struct HtmlTag : IHtmlTag
    {
        private readonly string mName;

        public HtmlTag(string name)
        {
            mName = name;
        }
        public HtmlTag(string name, bool needsClosure)
        {
            mName = name;
        }

        public bool Begin(StreamWriter writer)
        {
            writer.Write(String.Format("<{0}>", mName));
            return false;
        }

        public bool Begin(StreamWriter writer, HtmlAttribute attr)
        {
            writer.Write(String.Format("<{0}", mName));
            attr.Write(writer);
            writer.Write(">");
            return false;
        }

        public bool Begin(StreamWriter writer, HtmlAttribute[] attrs)
        {
            writer.Write(String.Format("<{0}", mName));
            foreach (HtmlAttribute attr in attrs)
                attr.Write(writer);
            writer.Write(">");
            return false;
        }

        public void End(StreamWriter writer)
        {
            writer.Write(String.Format("</{0}>", mName));
        }
    }

    public struct HtmlBrTag : IHtmlTag
    {
        private readonly string mName;

        public HtmlBrTag(string name)
        {
            mName = name;
        }

        public bool Begin(StreamWriter writer)
        {
            writer.Write(String.Format("<{0}>", mName));
            return true;
        }

        public bool Begin(StreamWriter writer, HtmlAttribute attr)
        {
            return true;
        }

        public bool Begin(StreamWriter writer, HtmlAttribute[] attrs)
        {
            return true;
        }

        public void End(StreamWriter writer)
        {
        }
    }

    public struct HtmlAttribute
    {
        private readonly string mName;
        private readonly string mValue;

        public HtmlAttribute(string name, string value)
        {
            mName = name;
            mValue = value;
        }

        public void Write(StreamWriter writer)
        {
            writer.Write(String.Format(" {0}={1}", mName, mValue));
        }
    }

    public struct HtmlComment : IHtmlTag
    {
        private readonly string mText;

        public HtmlComment(string text)
        {
            mText = text;
        }

        public bool Begin(StreamWriter writer)
        {
            writer.Write(String.Format("<!-- {0} -->", mText));
            return true;
        }

        public bool Begin(StreamWriter writer, HtmlAttribute attr)
        {
            return true;
        }

        public bool Begin(StreamWriter writer, HtmlAttribute[] attrs)
        {
            return true;
        }

        public void End(StreamWriter writer)
        {
        }
    }

    public interface IHtmlWriter : ITextWriter
    {
        /// <summary>
        /// Write                          <TAG>
        /// </summary>
        /// <param name="tag">The name of the HTML tag to start</param>
        void Write(IHtmlTag tag);

        /// <summary>
        /// Write                          <TAG ATTR1NAME=ATTR1VALUE ATTR2NAME=ATTR2VALUE ...>
        /// </summary>
        /// <param name="tag">The name of the HTML tag to start</param>
        /// <param name="attr">The attribute to add to the HTML tag</param>
        void Write(IHtmlTag tag, HtmlAttribute attr);

        /// <summary>
        /// Write                          <TAG ATTR1NAME=ATTR1VALUE ATTR2NAME=ATTR2VALUE ...>
        /// </summary>
        /// <param name="tag">The name of the HTML tag to start</param>
        /// <param name="attr">The list of attributes of the HTML tag</param>
        void Write(IHtmlTag tag, HtmlAttribute[] attr);

        /// <summary>
        /// Write N number of start tags,  <TAG1><TAG2><TAG3><TAG4>...
        /// </summary>
        /// <param name="tags">List of HTML start tags</param>
        void Write(params IHtmlTag[] tags);

        /// <summary>
        /// Will write                <TAG>text</TAG>
        /// </summary>
        /// <param name="tag">The name of the HTML tag</param>
        /// <param name="text">The text to write between the start and end tag</param>
        void Write(IHtmlTag tag, string text);

        /// <summary>
        /// Will write                <TAG ATTRNAME=ATTRVALUE>text</TAG>
        /// </summary>
        /// <param name="tag"></param>
        /// <param name="attr"></param>
        /// <param name="text"></param>
        void Write(IHtmlTag tag, HtmlAttribute attr, string text);
    }

    public class HtmlFileWriter : IHtmlWriter
    {
        private readonly Stack<IHtmlTag> mClosingTags = new Stack<IHtmlTag>();
        private readonly StreamWriter mWriter;

        public HtmlFileWriter(Stream stream)
        {
            mWriter = new StreamWriter(stream);
        }

        public void Write(string str)
        {
            if (mClosingTags.Count > 0)
                mWriter.Write(str);
        }

        public void WriteLine(string line)
        {
            if (mClosingTags.Count > 0)
                mWriter.WriteLine(line);
        }

        public void Write(IHtmlTag tag, string text)
        {
            tag.Begin(mWriter);
            mWriter.Write(text);
            tag.End(mWriter);
        }

        public void Write(IHtmlTag tag, HtmlAttribute attr, string text)
        {
            tag.Begin(mWriter, attr);
            mWriter.Write(text);
            tag.End(mWriter);
        }

        public void Write(IHtmlTag tag)
        {
            if (tag.Begin(mWriter) == false)
                mClosingTags.Push(tag);
        }

        public void Write(IHtmlTag tag, HtmlAttribute attr)
        {
            if (tag.Begin(mWriter, attr) == false)
                mClosingTags.Push(tag);
        }

        public void Write(IHtmlTag tag, HtmlAttribute[] attrs)
        {
            if (tag.Begin(mWriter, attrs) == false)
                mClosingTags.Push(tag);
        }

        public void Write(params IHtmlTag[] tags)
        {
            foreach (IHtmlTag t in tags)
                Write(t);
        }

        public void Close()
        {
            if (mWriter != null)
            {
                while (mClosingTags.Count > 0)
                    mClosingTags.Pop().End(mWriter);
                mWriter.Flush();
                mWriter.Close();
            }
        }
    }
}