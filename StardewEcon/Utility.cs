using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StardewEcon
{
    static class Utility
    {
        // Returns true if the string was truncated.
        public static bool DrawWrappedString(SpriteBatch b, SpriteFont font, string text, Rectangle bounds, Color color)
        {
            // This code is not terribly sturdy. Handle with care.
            string[] fullParagraphs = text.Split(new[]{'\n'}, StringSplitOptions.None);
            IEnumerable<string[]> paragraphs = fullParagraphs.Select(p => p.Split(new[]{' '}, StringSplitOptions.RemoveEmptyEntries));

            bool truncate = false;
            bool firstParagraph = true;
            StringBuilder outputText = new StringBuilder();
            foreach(string[] paragraph in paragraphs)
            {
                if( !firstParagraph )
                {
                    outputText.Append("\n");
                }

                // Check height
                Vector2 textSize;
                if (firstParagraph) {
                    textSize = font.MeasureString("J");
                }
                else {
                    textSize = font.MeasureString(outputText);
                }
                if( textSize.Y > bounds.Height )
                {
                    // If we're out of height, we're done.
                    truncate = true;
                    if( outputText.Length > 0 )
                    {
                        outputText.Remove(outputText.Length - 1, 1);
                    }
                    break;
                }

                // Append words of paragraph, wrapping when necessary
                bool firstWordInParagraph = true;
                foreach(string word in paragraph)
                {
                    // Attempt to append:
                    if (!firstWordInParagraph)
                    {
                        outputText.Append(" ");
                    }

                    outputText.Append(word);

                    // Check width:
                    textSize = font.MeasureString(outputText);
                    if( textSize.X > bounds.Width )
                    {
                        int spaceIndex = outputText.Length - 1 - word.Length;

                        // If this is the first word in the paragraph and we've
                        // already broken the width limit, we're kinda screwed.
                        if( firstWordInParagraph )
                        {
                            truncate = true;
                            outputText.Remove(spaceIndex, word.Length + 1);
                            break;
                        }

                        // Fix by wrapping
                        outputText.Replace(' ', '\n', spaceIndex, 1);

                        // Check width and height:
                        // If we've broken either limit, we have to truncate.
                        textSize = font.MeasureString(outputText);
                        if( (textSize.X > bounds.Width) || (textSize.Y > bounds.Height) )
                        {
                            truncate = true;
                            outputText.Remove(spaceIndex, word.Length + 1);
                            break;
                        }
                    }

                    firstWordInParagraph = false;
                }

                firstParagraph = false;

                if ( truncate )
                {
                    break;
                }
            }

            StardewValley.Utility.drawTextWithShadow(b, outputText.ToString(), font, new Vector2(bounds.X, bounds.Y), color);
            return truncate;
        }
    }
}
