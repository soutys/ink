using System;
using System.Collections.Generic;
using Ink.Runtime;

namespace Ink
{
    internal class CommandLineSplitter
    {
        public Story story { get; protected set; }
        protected Dictionary<string, int> namedNodes { get; set; }

        public CommandLineSplitter(Story story, Compiler compiler = null)
        {
            this.story = story;
            _compiler = compiler;
            this.namedNodes = new Dictionary<string, int>();
        }


        internal void FollowStory (ref List<object> jsonRoot) {

            while (story.canContinue) {
                EvaluateStory(ref jsonRoot);

                var choices = story.currentChoices;
                if (choices.Count == 1)
                {
                    jsonRoot.Add(choices[0].text);
                    story.ChooseChoiceIndex(0);
                    FollowStory(ref jsonRoot);

                    break;
                }
                else if (choices.Count > 1)
                {
                    string storyStateJSON = story.state.ToJson();
                    var _dict = new Dictionary<string, object>();
                    jsonRoot.Add(_dict);

                    for (var choiceIdx = 0; choiceIdx < choices.Count; ++choiceIdx)
                    {
                        story.ChooseChoiceIndex(choiceIdx);
                        var _jsonRoot = new List<object>();
                        _jsonRoot.Add(choices[choiceIdx].text);
                        _dict[choiceIdx.ToString()] = _jsonRoot;

                        FollowStory(ref _jsonRoot);
                        story.state.LoadJson(storyStateJSON);
                    }

                    break;
                }
            }
        }


        public void Split(Dictionary<string, object> jsonObject)
        {
            var jsonRoot = new List<object>();
            jsonObject["root"] = jsonRoot;
            FollowStory(ref jsonRoot);
        }


        void EvaluateStory (ref List<object> list)
        {
            while (story.canContinue) {

                story.Continue ();

                _compiler.RetrieveDebugSourceForLatestContent ();

                Ink.Runtime.Pointer prevPointer = story.state.previousPointer;
                Ink.Runtime.Pointer currPointer = story.state.currentPointer;

                list.Add(story.currentText);

                if (story.hasError) {
                    foreach (var errorMsg in story.currentErrors) {
                        Console.WriteLine (errorMsg, ConsoleColor.Red);
                    }
                }

                if (story.hasWarning) {
                    foreach (var warningMsg in story.currentWarnings) {
                        Console.WriteLine (warningMsg, ConsoleColor.Blue);
                    }
                }

                story.ResetErrors ();
            }
        }

        readonly Compiler _compiler;
    }


}

