using System;
using System.IO;

namespace Paragon.Foundation.LivePhoto.HtmlHelpers
{
    public class RenderElement : IDisposable
    {
        private readonly TextWriter _writer;
        private readonly string _firstPart;
        private readonly string _lastPart;

        public RenderElement(TextWriter writer, string firstPart, string lastPart)
        {
            _writer = writer;
            _firstPart = firstPart;
            _lastPart = lastPart;
            _writer.Write(_firstPart);
        }

        public void Dispose()
        {
            _writer.Write(_lastPart);
        }
    }
}