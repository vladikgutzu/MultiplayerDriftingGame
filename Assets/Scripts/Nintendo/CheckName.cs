using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace WizardsOwl
{
    public class CheckName : MonoBehaviour
    {
         public static bool CheckProfanityWordsSample(string inputText)
        {
            if (string.IsNullOrWhiteSpace(inputText))
                return false;

#if UNITY_SWITCH && !UNITY_EDITOR
            if (inputText.Length > 0)
            {
                // stringBuilder.AppendFormat("\n### Profanity text check sample.\n");
                // Debug.LogError("NINTENDO_PROFANITY START");
                using (var filter = new nn.ngc.ProfanityFilter())
                {
                    uint version = filter.GetContentVersion();
                    if (version == 0)
                    {

                        Debug.LogError("NINTENDO_PROFANITY The pattern list version is invalid");
                        //   stringBuilder.AppendFormat("The pattern list version is invalid.\n");
                        return false;
                    }
                    else
                    {
                        // stringBuilder.AppendFormat("The pattern list version is {0}.\n", version);

                        // Assuming that text data can move between regions, reference the
                        // system's current region and language settings and check for profanity.

                        string[] words = inputText.Split(' ');
                        var patterns = nn.ngc.ProfanityFilter.PatternList.Japanese | nn.ngc.ProfanityFilter.PatternList.SimplifiedChinese | nn.ngc.ProfanityFilter.PatternList.AmericanEnglish | nn.ngc.ProfanityFilter.PatternList.German | nn.ngc.ProfanityFilter.PatternList.Spanish;

                        nn.ngc.ProfanityFilter.PatternList[] checkResults = new nn.ngc.ProfanityFilter.PatternList[words.Length];

                        nn.Result result = filter.CheckProfanityWords(checkResults, patterns, words);
                        if (!result.IsSuccess())
                        {

                            Debug.LogError("NINTENDO_PROFANITY Failed the check processing for profanity strings");
                            //   stringBuilder.AppendFormat("Failed the check processing for profanity strings.\n");
                            return false;
                        }
                        else
                        {

                            for (int i = 0; i < words.Length; i++)
                            {
                                if (checkResults[i] == 0)
                                {
                                    //   stringBuilder.AppendFormat("\"{0}\" does not have a match in the pattern list.\n", words[i]);
                                }
                                else
                                {

                                    Debug.LogError
                                    ("NINTENDO_PROFANITY " + words[i] + " has a match in the pattern list: " + checkResults[i].ToString() + "\n");
                                    return false;
                                    //  stringBuilder.AppendFormat("\"{0}\" has a match in the pattern list: [{1}]\n", words[i], checkResults[i].ToString());
                                }
                            }

                            return true;

                        }
                    }
                }

            }
            else
            {
                Debug.LogError("NINTENDO_PROFANITY Length = 0");
                return false;
            }
#else
            return true;
#endif
        }
    }
}
