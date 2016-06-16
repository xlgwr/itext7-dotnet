/*

This file is part of the iText (R) project.
Copyright (c) 1998-2016 iText Group NV
Authors: Bruno Lowagie, Paulo Soares, et al.

This program is free software; you can redistribute it and/or modify
it under the terms of the GNU Affero General Public License version 3
as published by the Free Software Foundation with the addition of the
following permission added to Section 15 as permitted in Section 7(a):
FOR ANY PART OF THE COVERED WORK IN WHICH THE COPYRIGHT IS OWNED BY
ITEXT GROUP. ITEXT GROUP DISCLAIMS THE WARRANTY OF NON INFRINGEMENT
OF THIRD PARTY RIGHTS

This program is distributed in the hope that it will be useful, but
WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY
or FITNESS FOR A PARTICULAR PURPOSE.
See the GNU Affero General Public License for more details.
You should have received a copy of the GNU Affero General Public License
along with this program; if not, see http://www.gnu.org/licenses or write to
the Free Software Foundation, Inc., 51 Franklin Street, Fifth Floor,
Boston, MA, 02110-1301 USA, or download the license from the following URL:
http://itextpdf.com/terms-of-use/

The interactive user interfaces in modified source and object code versions
of this program must display Appropriate Legal Notices, as required under
Section 5 of the GNU Affero General Public License.

In accordance with Section 7(b) of the GNU Affero General Public License,
a covered work must retain the producer line in every PDF that is created
or manipulated using iText.

You can be released from the requirements of the license by purchasing
a commercial license. Buying such a license is mandatory as soon as you
develop commercial activities involving the iText software without
disclosing the source code of your own applications.
These activities include: offering paid services to customers as an ASP,
serving PDFs on the fly in a web application, shipping iText with a closed
source product.

For more information, please contact iText Software Corp. at this
address: sales@itextpdf.com
*/
using System;
using System.Collections.Generic;
using System.Text;
using iText.IO.Util;

namespace iText.IO.Font.Otf {
    public class GlyphLine {
        protected internal IList<Glyph> glyphs;

        protected internal IList<GlyphLine.ActualText> actualText;

        public int start;

        public int end;

        public int idx;

        public GlyphLine() {
            this.glyphs = new List<Glyph>();
        }

        public GlyphLine(IList<Glyph> glyphs) {
            this.glyphs = glyphs;
            this.start = 0;
            this.end = glyphs.Count;
        }

        public GlyphLine(IList<Glyph> glyphs, int start, int end) {
            this.glyphs = glyphs;
            this.start = start;
            this.end = end;
        }

        protected internal GlyphLine(IList<Glyph> glyphs, IList<GlyphLine.ActualText> actualText, int start, int end
            )
            : this(glyphs, start, end) {
            this.actualText = actualText;
        }

        public GlyphLine(iText.IO.Font.Otf.GlyphLine other) {
            this.glyphs = other.glyphs;
            this.actualText = other.actualText;
            this.start = other.start;
            this.end = other.end;
            this.idx = other.idx;
        }

        public GlyphLine(iText.IO.Font.Otf.GlyphLine other, int start, int end) {
            this.glyphs = other.glyphs.SubList(start, end);
            if (other.actualText != null) {
                this.actualText = other.actualText.SubList(start, end);
            }
            this.start = 0;
            this.end = end - start;
            this.idx = other.idx - start;
        }

        public virtual String ToUnicodeString(int start, int end) {
            ActualTextIterator iter = new ActualTextIterator(this, start, end);
            StringBuilder str = new StringBuilder();
            while (iter.HasNext()) {
                GlyphLine.GlyphLinePart part = iter.Next();
                if (part.actualText != null) {
                    str.Append(part.actualText);
                }
                else {
                    for (int i = part.start; i < part.end; i++) {
                        if (glyphs[i].GetChars() != null) {
                            str.Append(glyphs[i].GetChars());
                        }
                        else {
                            if (glyphs[i].HasValidUnicode()) {
                                str.Append(TextUtil.ConvertFromUtf32((int)glyphs[i].GetUnicode()));
                            }
                        }
                    }
                }
            }
            return str.ToString();
        }

        public virtual iText.IO.Font.Otf.GlyphLine Copy(int left, int right) {
            iText.IO.Font.Otf.GlyphLine glyphLine = new iText.IO.Font.Otf.GlyphLine();
            glyphLine.start = 0;
            glyphLine.end = right - left;
            glyphLine.glyphs = new List<Glyph>(glyphs.SubList(left, right));
            glyphLine.actualText = actualText == null ? null : new List<GlyphLine.ActualText>(actualText.SubList(left, 
                right));
            return glyphLine;
        }

        public virtual Glyph Get(int index) {
            return glyphs[index];
        }

        public virtual Glyph Set(int index, Glyph glyph) {
            return glyphs[index] = glyph;
        }

        public virtual void Add(Glyph glyph) {
            glyphs.Add(glyph);
            if (actualText != null) {
                actualText.Add(null);
            }
        }

        public virtual void Add(int index, Glyph glyph) {
            glyphs.Add(index, glyph);
            if (actualText != null) {
                actualText.Add(index, null);
            }
        }

        public virtual void SetGlyphs(IList<Glyph> replacementGlyphs) {
            glyphs = new List<Glyph>(replacementGlyphs);
            start = 0;
            end = replacementGlyphs.Count;
            actualText = null;
        }

        public virtual void ReplaceContent(iText.IO.Font.Otf.GlyphLine other) {
            glyphs.Clear();
            glyphs.AddAll(other.glyphs);
            if (actualText != null) {
                actualText.Clear();
            }
            if (other.actualText != null) {
                if (actualText == null) {
                    actualText = new List<GlyphLine.ActualText>();
                }
                actualText.AddAll(other.actualText);
            }
            start = other.start;
            end = other.end;
        }

        public virtual int Size() {
            return glyphs.Count;
        }

        public virtual void SubstituteManyToOne(OpenTypeFontTableReader tableReader, int lookupFlag, int rightPartLen
            , int substitutionGlyphIndex) {
            OpenTableLookup.GlyphIndexer gidx = new OpenTableLookup.GlyphIndexer();
            gidx.line = this;
            gidx.idx = idx;
            StringBuilder chars = new StringBuilder();
            Glyph currentGlyph = glyphs[idx];
            if (currentGlyph.GetChars() != null) {
                chars.Append(currentGlyph.GetChars());
            }
            else {
                if (currentGlyph.HasValidUnicode()) {
                    chars.Append(TextUtil.ConvertFromUtf32((int)currentGlyph.GetUnicode()));
                }
            }
            for (int j = 0; j < rightPartLen; ++j) {
                gidx.NextGlyph(tableReader, lookupFlag);
                currentGlyph = glyphs[gidx.idx];
                if (currentGlyph.GetChars() != null) {
                    chars.Append(currentGlyph.GetChars());
                }
                else {
                    if (currentGlyph.HasValidUnicode()) {
                        chars.Append(TextUtil.ConvertFromUtf32((int)currentGlyph.GetUnicode()));
                    }
                }
                RemoveGlyph(gidx.idx--);
            }
            char[] newChars = new char[chars.Length];
            chars.GetChars(0, chars.Length, newChars, 0);
            Glyph newGlyph = tableReader.GetGlyph(substitutionGlyphIndex);
            newGlyph.SetChars(newChars);
            glyphs[idx] = newGlyph;
            end -= rightPartLen;
        }

        public virtual void SubstituteOneToOne(OpenTypeFontTableReader tableReader, int substitutionGlyphIndex) {
            Glyph oldGlyph = glyphs[idx];
            Glyph newGlyph = tableReader.GetGlyph(substitutionGlyphIndex);
            if (oldGlyph.GetChars() != null) {
                newGlyph.SetChars(oldGlyph.GetChars());
            }
            else {
                if (newGlyph.HasValidUnicode()) {
                    newGlyph.SetChars(TextUtil.ConvertFromUtf32((int)newGlyph.GetUnicode()));
                }
                else {
                    if (oldGlyph.HasValidUnicode()) {
                        newGlyph.SetChars(TextUtil.ConvertFromUtf32((int)oldGlyph.GetUnicode()));
                    }
                }
            }
            glyphs[idx] = newGlyph;
        }

        public virtual void SubstituteOneToMany(OpenTypeFontTableReader tableReader, int[] substGlyphIds) {
            int substCode = substGlyphIds[0];
            //sequence length shall be at least 1
            Glyph glyph = tableReader.GetGlyph(substCode);
            glyphs[idx] = glyph;
            if (substGlyphIds.Length > 1) {
                IList<Glyph> additionalGlyphs = new List<Glyph>(substGlyphIds.Length - 1);
                for (int i = 1; i < substGlyphIds.Length; ++i) {
                    substCode = substGlyphIds[i];
                    glyph = tableReader.GetGlyph(substCode);
                    additionalGlyphs.Add(glyph);
                }
                AddAllGlyphs(idx + 1, additionalGlyphs);
                idx += substGlyphIds.Length - 1;
                end += substGlyphIds.Length - 1;
            }
        }

        public virtual iText.IO.Font.Otf.GlyphLine Filter(GlyphLine.IGlyphLineFilter filter) {
            bool anythingFiltered = false;
            IList<Glyph> filteredGlyphs = new List<Glyph>(end - start);
            IList<GlyphLine.ActualText> filteredActualText = actualText != null ? new List<GlyphLine.ActualText>(end -
                 start) : null;
            for (int i = start; i < end; i++) {
                if (filter.Accept(glyphs[i])) {
                    filteredGlyphs.Add(glyphs[i]);
                    if (filteredActualText != null) {
                        filteredActualText.Add(actualText[i]);
                    }
                }
                else {
                    anythingFiltered = true;
                }
            }
            if (anythingFiltered) {
                return new iText.IO.Font.Otf.GlyphLine(filteredGlyphs, filteredActualText, 0, filteredGlyphs.Count);
            }
            else {
                return this;
            }
        }

        public virtual void SetActualText(int left, int right, String text) {
            if (this.actualText == null) {
                this.actualText = new List<GlyphLine.ActualText>(glyphs.Count);
                for (int i = 0; i < glyphs.Count; i++) {
                    this.actualText.Add(null);
                }
            }
            GlyphLine.ActualText actualText = new GlyphLine.ActualText(text);
            for (int i_1 = left; i_1 < right; i_1++) {
                this.actualText[i_1] = actualText;
            }
        }

        public virtual IEnumerator<GlyphLine.GlyphLinePart> Iterator() {
            return new ActualTextIterator(this);
        }

        private void RemoveGlyph(int index) {
            glyphs.JRemoveAt(index);
            if (actualText != null) {
                actualText.JRemoveAt(index);
            }
        }

        private void AddAllGlyphs(int index, IList<Glyph> additionalGlyphs) {
            glyphs.AddAll(index, additionalGlyphs);
            if (actualText != null) {
                for (int i = 0; i < additionalGlyphs.Count; i++) {
                    this.actualText.Add(index, null);
                }
            }
        }

        public class GlyphLinePart {
            public int start;

            public int end;

            public String actualText;

            public GlyphLinePart(int start, int end, String actualText) {
                this.start = start;
                this.end = end;
                this.actualText = actualText;
            }
        }

        public interface IGlyphLineFilter {
            bool Accept(Glyph glyph);
        }

        protected internal class ActualText {
            public ActualText(String value) {
                this.value = value;
            }

            public String value;
        }
    }
}