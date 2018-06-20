﻿using System.Text.RegularExpressions;

namespace WildcardPattern
{
  public static class WildcardPattern
  {
    public static bool IsMatchOptimized(
      string input,
      string pattern,
      char singleWildcard = '?',
      char multipleWildcard = '*')
    {
      var inputPosStack =
        new int[(input.Length + 1) *
                (pattern.Length + 1)]; // Stack containing input positions that should be tested for further matching
      var
        patternPosStack =
          new int[inputPosStack
            .Length]; // Stack containing pattern positions that should be tested for further matching
      var stackPos = -1; // Points to last occupied entry in stack; -1 indicates that stack is empty
      var
        pointTested =
          new bool[input.Length + 1,
            pattern.Length + 1]; // Each true value indicates that input position vs. pattern position has been tested

      var inputPos = 0; // Position in input matched up to the first multiple wildcard in pattern
      var patternPos = 0; // Position in pattern matched up to the first multiple wildcard in pattern

      // Match beginning of the string until first multiple wildcard in pattern
      while (inputPos < input.Length && patternPos < pattern.Length && pattern[patternPos] != multipleWildcard &&
             (input[inputPos] == pattern[patternPos] || pattern[patternPos] == singleWildcard))
      {
        inputPos++;
        patternPos++;
      }

      // Push this position to stack if it points to end of pattern or to a general wildcard
      if (patternPos == pattern.Length || pattern[patternPos] == multipleWildcard)
      {
        pointTested[inputPos, patternPos] = true;
        inputPosStack[++stackPos] = inputPos;
        patternPosStack[stackPos] = patternPos;
      }

      var matched = false;

      // Repeat matching until either string is matched against the pattern or no more parts remain on stack to test
      while (stackPos >= 0 && !matched)
      {
        inputPos = inputPosStack[stackPos]; // Pop input and pattern positions from stack
        patternPos =
          patternPosStack[stackPos--]; // Matching will succeed if rest of the input string matches rest of the pattern

        if (inputPos == input.Length && patternPos == pattern.Length)
          matched = true; // Reached end of both pattern and input string, hence matching is successful
        else
        {
          // First character in next pattern block is guaranteed to be multiple wildcard
          // So skip it and search for all matches in value string until next multiple wildcard character is reached in pattern

          for (var curInputStart = inputPos; curInputStart < input.Length; curInputStart++)
          {
            var curInputPos = curInputStart;
            var curPatternPos = patternPos + 1;

            if (curPatternPos == pattern.Length)
            {
              // Pattern ends with multiple wildcard, hence rest of the input string is matched with that character
              curInputPos = input.Length;
            }
            else
            {
              while (curInputPos < input.Length && curPatternPos < pattern.Length &&
                     pattern[curPatternPos] != multipleWildcard &&
                     (input[curInputPos] == pattern[curPatternPos] || pattern[curPatternPos] == singleWildcard))
              {
                curInputPos++;
                curPatternPos++;
              }
            }

            // If we have reached next multiple wildcard character in pattern without breaking the matching sequence, then we have another candidate for full match
            // This candidate should be pushed to stack for further processing
            // At the same time, pair (input position, pattern position) will be marked as tested, so that it will not be pushed to stack later again
            if ((curPatternPos != pattern.Length || curInputPos != input.Length) &&
                (curPatternPos >= pattern.Length || pattern[curPatternPos] != multipleWildcard) ||
                pointTested[curInputPos, curPatternPos]) continue;
            pointTested[curInputPos, curPatternPos] = true;
            inputPosStack[++stackPos] = curInputPos;
            patternPosStack[stackPos] = curPatternPos;
          }
        }
      }

      return matched;
    }

    public static bool IsMatchRegexBased(
      string input,
      string pattern,
      char singleWildcard = '?',
      char multipleWildcard = '*')
    {
      var regex =
        $"^{Regex.Escape(pattern).Replace($"\\{multipleWildcard}", ".*").Replace($"\\{singleWildcard}", ".")}$";
      return Regex.IsMatch(input, regex, RegexOptions.IgnoreCase);
    }
  }
}