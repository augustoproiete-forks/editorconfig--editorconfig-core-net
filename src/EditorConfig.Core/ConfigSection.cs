using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace EditorConfig.Core
{
	/// <summary>
	/// Represents an ini section within the editorconfig file
	/// </summary>
	public class ConfigSection : IReadOnlyDictionary<string, string>
	{
		private readonly Dictionary<string, string> _backingDictionary;

		private static readonly Dictionary<string, string> DefaultGlobalDictionary = new Dictionary<string, string>();
		public ConfigSection() => _backingDictionary = DefaultGlobalDictionary;

		public ConfigSection(string name, string configDirectory, Dictionary<string, string> backingDictionary)
		{
			Glob = FixGlob(name, configDirectory);
			_backingDictionary = backingDictionary ?? new Dictionary<string, string>();
			this.ParseKnownProperties();
		} 
		
		/// <summary> The glob pattern this section describes</summary>
		public string Glob { get; }
		
		/// <summary>
		/// set to tab or space to use hard tabs or soft tabs respectively.
		/// </summary>
		public IndentStyle? IndentStyle { get; private set; }

		/// <summary>
		/// a whole number defining the number of columns used for each indentation level and the width of soft tabs (when supported). 
		/// When set to tab, the value of tab_width (if specified) will be used.
		/// </summary>
		public IndentSize IndentSize { get; private set; }
		
		/// <summary>
		/// a whole number defining the number of columns used to represent a tab character. 
		/// This defaults to the value of indent_size and doesn't usually need to be specified.
		/// </summary>
		public int? TabWidth { get; private set; }
		
		/// <summary>
		/// set to lf, cr, or crlf to control how line breaks are represented.
		/// </summary>
		public EndOfLine? EndOfLine { get; private set; }
		
		/// <summary>
		/// set to latin1, utf-8, utf-8-bom, utf-16be or utf-16le to control the character set. Use of utf-8-bom is discouraged.
		/// </summary>
		public Charset? Charset { get; private set; }
		
		/// <summary>
		/// set to true to remove any whitespace characters preceding newline characters and false to ensure it doesn't.
		/// </summary>
		public bool? TrimTrailingWhitespace { get; private set; }
		
		/// <summary>
		/// set to true ensure file ends with a newline when saving and false to ensure it doesn't.
		/// </summary>
		public bool? InsertFinalNewline { get; private set; }
		
		/// <summary>
		/// Forces hard line wrapping after the amount of characters specified
		/// </summary>
		public int? MaxLineLength { get; private set; }

		
		private static string FixGlob(string glob, string directory)
		{
			switch (glob.IndexOf('/'))
			{
				case -1: glob = "**/" + glob; break;
				case 0: glob = glob.Substring(1); break;
			}
			directory = directory.Replace(@"\", "/");
			if (!directory.EndsWith("/")) directory += "/";

			return directory + glob;
		}
		
		private void ParseKnownProperties()
		{
			this.ParseIndentStyle();
			this.ParseIndentSize();
			this.ParseTabWidth();
			this.ParseEndOfLine();
			this.ParseCharset();
			this.ParseTrimTrailingWhitespace();
			this.ParseInsertFinalNewline();
			this.ParseMaxLineLength();
		}

		private void ParseIndentStyle()
		{
			if (!this.TryGetValue("indent_style", out var indentStyle)) return;

			switch (indentStyle)
			{
				case "space":
					this.IndentStyle = EditorConfig.Core.IndentStyle.Space;
					return;
				case "tab":
					this.IndentStyle = EditorConfig.Core.IndentStyle.Tab;
					return;
			}
		}

		private void ParseIndentSize()
		{
			if (!this.TryGetValue("indent_size", out var indentSize)) return;

			switch (indentSize)
			{
				case "unset":
					this.IndentSize = IndentSize.Unset;
					return;
				case "tab":
					this.IndentSize = IndentSize.Tab;
					return;
				default:
					int size;
					if (int.TryParse(indentSize, out size) && size > 0)
						this.IndentSize = IndentSize.Columns(size);
					return;
			}
		}

		private void ParseMaxLineLength()
		{
			if (!this.TryGetValue("max_line_length", out var maxLineLength)) return;

			if (int.TryParse(maxLineLength, out var length) && length > 0)
				this.MaxLineLength = length;
		}

		private void ParseTabWidth()
		{
			if (!this.TryGetValue("tab_width", out var tabWidth)) return;

			if (int.TryParse(tabWidth, out var width) && width > 0)
				this.TabWidth = width;
		}

		private void ParseEndOfLine()
		{
			if (!this.TryGetValue("end_of_line", out var endOfLine)) return;

			switch (endOfLine)
			{
				case "lf":
					this.EndOfLine = EditorConfig.Core.EndOfLine.LF;
					return;
				case "cr":
					this.EndOfLine = EditorConfig.Core.EndOfLine.CR;
					return;
				case "crlf":
					this.EndOfLine = EditorConfig.Core.EndOfLine.CRLF;
					return;
			}
		}

		private void ParseCharset()
		{
			if (!this.TryGetValue("charset", out var charset)) return;

			switch (charset)
			{
				case "latin1":
					this.Charset = EditorConfig.Core.Charset.Latin1;
					return;
				case "utf-16be":
					this.Charset = EditorConfig.Core.Charset.UTF16BE;
					return;
				case "utf-16le":
					this.Charset = EditorConfig.Core.Charset.UTF16LE;
					return;
				case "utf-8":
					this.Charset = EditorConfig.Core.Charset.UTF8;
					return;
				case "utf-8-bom":
					this.Charset = EditorConfig.Core.Charset.UTF8BOM;
					return;
			}
		}

		private void ParseTrimTrailingWhitespace()
		{
			if (!this.TryGetValue("trim_trailing_whitespace", out var trimTrailingWhitespace)) return;

			if (bool.TryParse(trimTrailingWhitespace, out var trim))
				this.TrimTrailingWhitespace = trim;
		}

		private void ParseInsertFinalNewline()
		{
			if (!this.TryGetValue("insert_final_newline", out var insertFinalNewline)) return;

			if (bool.TryParse(insertFinalNewline, out var insert))
				this.InsertFinalNewline = insert;
		}

		IEnumerator<KeyValuePair<string, string>> IEnumerable<KeyValuePair<string, string>>.GetEnumerator() => 
			((IEnumerable<KeyValuePair<string, string>>) _backingDictionary).GetEnumerator();

		IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable) _backingDictionary).GetEnumerator();
		
		public int Count => _backingDictionary.Count;
		public bool ContainsKey(string key) => _backingDictionary.ContainsKey(key);
		public bool TryGetValue(string key, out string value) =>
			_backingDictionary.TryGetValue(key, out value);
		public string this[string key] => _backingDictionary[key];
		public IEnumerable<string> Keys => _backingDictionary.Keys;
		public IEnumerable<string> Values => _backingDictionary.Values;
	}
}