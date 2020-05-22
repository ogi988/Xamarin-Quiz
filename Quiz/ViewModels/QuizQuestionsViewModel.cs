﻿using Java.Lang;
using Newtonsoft.Json;
using Quiz.Models;
using Quiz.Services;
using Quiz.Views;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.Forms;


namespace Quiz.ViewModels
{
    class QuizQuestionsViewModel : INotifyPropertyChanged
    {
        public bool Stop { get; set; }
        public List<UserScoreList> userScoreList { get; set; }
        public List<QuestionList> finalQuestionList { get; set; }
        public string Username { get; set; }
        private readonly Api _api = new Api();
        public event PropertyChangedEventHandler PropertyChanged;
        private bool _startQuiz = false;

        private int _score = 0;
        public int QuestionNumber { get; set; } = 0;
        public int Score { get { return _score; } set { _score = value; PropertyChanged(this, new PropertyChangedEventArgs(nameof(Score))); } }  
        
        public bool StartQuiz {
            get
            {
                return _startQuiz;
            }
            set
            {
                _startQuiz = value;
            }
        }
        private long _difficulty;
        public long Difficulty
        {
            get { return _difficulty; }
            set
            {
                _difficulty = value;
                PropertyChanged(this, new PropertyChangedEventArgs(nameof(Difficulty)));
            }
        }
        private string _correctAnswer;
        public string CorrectAnswer
        {
            get { return _correctAnswer; }
            set
            {
                _correctAnswer = value;
                PropertyChanged(this, new PropertyChangedEventArgs(nameof(CorrectAnswer)));
            }
        }
        private string _question;
        public string Question
        {
            get { return _question; }
            set
            {
                _question = value;
                PropertyChanged(this, new PropertyChangedEventArgs(nameof(Question)));
            }
        }

        private string _answer1;
        public string Answer1
        {
            get { return this._answer1; }
            set
            {
                _answer1 = value;
                PropertyChanged(this, new PropertyChangedEventArgs(nameof(Answer1)));
            }
        }



        private string _answer2;
        public string Answer2
        {
            get { return this._answer2; }
            set
            {
                _answer2 = value;
                PropertyChanged(this, new PropertyChangedEventArgs(nameof(Answer2)));
            }
        }

        public List<QuestionList> questionList { get; set; }
        public List<int> RandomNums { get; set; }

        private string _answer3;
        public string Answer3
        {
            get { return this._answer3; }
            set
            {
                _answer3 = value;
                PropertyChanged(this, new PropertyChangedEventArgs(nameof(Answer3)));
            }
        }
        private string _answer4;
        public string Answer4
        {
            get { return this._answer4; }
            set
            {
                _answer4 = value;
                PropertyChanged(this, new PropertyChangedEventArgs(nameof(Answer4)));
            }
        }
        private string _time;
        public string Time
        {
            get { return this._time; }
            set
            {
                _time = value;
                PropertyChanged(this, new PropertyChangedEventArgs(nameof(Time)));
            }
        }
        public TimeSpan StartTime { get; set; }

        
        
        
        
        public ICommand Check => new Command(async(btnText) =>
        {
            Username = Settings.Settings.Username;

            if(QuestionNumber == 5)
            {
                var model = new UserScore
                {
                    Username = Username,
                    Score = Score
                };

                HttpClientHandler clientHandler = new HttpClientHandler();
                clientHandler.ServerCertificateCustomValidationCallback = (senders, cert, chain, sslPolicyErrors) => { return true; };
                HttpClient client = new HttpClient(clientHandler);                
                var json = JsonConvert.SerializeObject(model);
                HttpContent httpContent = new StringContent(json);
                httpContent.Headers.ContentType = new MediaTypeHeaderValue("application/json");

                var response = await client.PostAsync(Constants.Api + "api/UserScores", httpContent);

                StopTimer();
                Application.Current.MainPage = new NavigationPage(new EndQuiz(Score));

            }
            bool result = checkAnswer(btnText.ToString());
            if (result)
            {
                
                Score += Convert.ToInt32(Difficulty);
            }
            var newQuestion =  NewQuestion();
            List<string> answers = new List<string>();
            answers.Add(newQuestion.Answer1);
            answers.Add(newQuestion.Answer2);
            answers.Add(newQuestion.Answer3);
            answers.Add(newQuestion.CorrectAnswer);

            var shuffled = answers.OrderBy(x => Guid.NewGuid()).ToList();
            CorrectAnswer = newQuestion.CorrectAnswer;
            
            Answer1 = shuffled[0];
            Answer2 = shuffled[1];
            Answer3 = shuffled[2];
            Answer4 = shuffled[3];
            Question = newQuestion.Text;
            QuestionNumber++;
            
            
            
            
            

        });
        
        public bool checkAnswer(string btnText)
        {
            if (btnText == CorrectAnswer)
            {
                return true;
            }
            else
                return false;
        }
        public async Task<bool> GetQuestions()
        {
            
            var request = new HttpRequestMessage(HttpMethod.Get, Constants.Api + "api/Questions");
            HttpClientHandler clientHandler = new HttpClientHandler();
            clientHandler.ServerCertificateCustomValidationCallback = (senders, cert, chain, sslPolicyErrors) => { return true; };

            HttpClient client = new HttpClient(clientHandler);
            
            var response = await client.SendAsync(request);
            var content = await response.Content.ReadAsStringAsync();
            var FinalQuestionList = new List<QuestionList>();
            questionList = JsonConvert.DeserializeObject<List<QuestionList>>(content);
            int maxNumber = (from x in userScoreList where x.Username == Settings.Settings.Username select x.Score).Max();
            int sevenQuestions = maxNumber / 10;
            Random rnd = new Random();
            for (int i = 0; i < 7; i++)
            {
                int random = rnd.Next(sevenQuestions, sevenQuestions + 1);
                FinalQuestionList.AddRange(from x in questionList where x.Difficulty == random select x);


            }
            for (int i = 0; i < 3; i++)
            {
                if (maxNumber > 80)
                {

                    int random = rnd.Next(8, 10);
                    FinalQuestionList.AddRange(from x in questionList
                                               where x.Difficulty == random
                                               select x);
                }
                else
                {
                    int random = rnd.Next(sevenQuestions + 2, 10);
                    FinalQuestionList.AddRange(from x in questionList
                                               where x.Difficulty == random
                                               select x);
                }


            }
            finalQuestionList = FinalQuestionList;


            return true;
            
        }
        public QuestionList NewQuestion()
        {
            
            Random rnd = new Random();
            int num;
            do
            {
                num = rnd.Next(1, finalQuestionList.Count);
            } while (RandomNums.Contains(num));
            RandomNums.Add(num);
            return finalQuestionList[num];
        }
        public async void Start()
        {
            StartTime = TimeSpan.FromSeconds(60);
            Time = StartTime.ToString();
            RandomNums = new List<int>();
            
            bool getScores = await GetUserScores();
            bool getQuestions = await GetQuestions();
            
            if (getQuestions && getScores)
            {
                var newQuestion = NewQuestion();
                List<string> answers = new List<string>();
                answers.Add(newQuestion.Answer1);
                answers.Add(newQuestion.Answer2);
                answers.Add(newQuestion.Answer3);
                answers.Add(newQuestion.CorrectAnswer);

                var shuffled = answers.OrderBy(x => Guid.NewGuid()).ToList();
                CorrectAnswer = newQuestion.CorrectAnswer;

                Answer1 = shuffled[0];
                Answer2 = shuffled[1];
                Answer3 = shuffled[2];
                Answer4 = shuffled[3];
                Question = newQuestion.Text;
                Difficulty = newQuestion.Difficulty;
                Stop = false;
                Device.StartTimer(TimeSpan.FromSeconds(1), () =>
                {
                    if (Stop)
                    {
                        return false;
                    }
                    else if (StartTime.TotalSeconds > 0)
                    {
                        StartTime = StartTime - TimeSpan.FromSeconds(1);
                        Time = StartTime.ToString();

                        return true;
                    }
                    else
                    {
                        Application.Current.MainPage = new NavigationPage(new EndQuiz(Score));
                        return false;

                    }
                });


                StartQuiz = true;
                
            }

        }

        public void StopTimer()
        {
            Stop = true;
        }
        public async Task<bool> GetUserScores()
        {

            var request = new HttpRequestMessage(HttpMethod.Get, Constants.Api + "api/UserScores");
            HttpClientHandler clientHandler = new HttpClientHandler();
            clientHandler.ServerCertificateCustomValidationCallback = (senders, cert, chain, sslPolicyErrors) => { return true; };

            HttpClient client = new HttpClient(clientHandler);

            var response = await client.SendAsync(request);
            var content = await response.Content.ReadAsStringAsync();
            userScoreList = JsonConvert.DeserializeObject<List<UserScoreList>>(content);


            return true;

        }

    }
}
