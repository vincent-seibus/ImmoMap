function SearchTextService() {

    this.LevenshteinDistance = function(s,t)
                {
                    // degenerate cases
                    if (s == t) return 0;
                    if (s.length == 0) return t.length;
                    if (t.length == 0) return s.length;

                    // create two work vectors of integer distances
                    //int[] v0 = new int[t.Length + 1];
                    //int[] v1 = new int[t.Length + 1];
                    var v0 = [];
                    var v1 = [];

                    // initialize v0 (the previous row of distances)
                    // this row is A[0][i]: edit distance for an empty s
                    // the distance is just the number of characters to delete from t
                    for (var i = 0; i < t.length + 1; i++)
                    {
                        v0[i] = i;
                    }

                    for (var i = 0; i < s.length; i++)
                    {
                        // calculate v1 (current row distances) from the previous row v0
                        
                        // first element of v1 is A[i+1][0]
                        //   edit distance is delete (i+1) chars from s to match empty t
                        v1[0] = i + 1;

                        // use formula to fill in the rest of the row
                        for (var j = 0; j < t.length; j++)
                        {
                            var cost = (s[i] == t[j]) ? 0 : 1;
                            v1[j + 1] = Math.min(v1[j] + 1, v0[j + 1] + 1, v0[j] + cost);
                        }

                        // copy v1 (current row) to v0 (previous row) for next iteration
                        for (var j = 0; j < t.length + 1; j++)
                            v0[j] = v1[j];
                    }

                    return v1[t.length];
    }
  
    this.LevenshteinDistanceLogarithmic = function(s,t)
    {
        return 1 / Math.log( 1 + this.LevenshteinDistance(s,t));
    }
}

function UserManager()
{
    this.getSpaceIdFromUser = function(user)
    {
        for(var i = 0; i < user.Claims.length; i++)
        {
            if(user.Claims[i].ClaimType == "SpaceId")
                return user.Claims[i].ClaimValue;
        }
    }
}

// This service is used to pass object from one controller to another
function ObjectManager($location, $http)
{
    var object = {};
    return {
            getObject: function () {
                return object;
            },
            setObject: function(value) {
                object = value;
            },  
            getInfo: function (info) {
                if (info != null && info != undefined && info != "") {
                    var InfoParsed = JSON.parse(info);
                    return InfoParsed;
                }
                else
                    return null;
            },
            GetObjectById: function (url, Id) {
                object = value;
                if (object != null && object != undefined && object.Id != undefined && object.Id != null) {
                    $location.search("Id", object.Id);
                }
                else {
                    if ($location.search().Id != null && $location.search().Id != undefined) {
                        $http.get(url + "?id=" + $location.search().Id).success(function (dataout) { object = dataout; });
                    }                
                }
            }
        }; 
}

// these service is the quiz result generator
function QuizEngine() {
    var quiz = {};
    var scoreTotal = null;
    var scoreMax = null;  
    var allResponses = [];
    var skillScores = [];
    return {
        getQuiz: function () {
            return quiz;
        },
        setQuiz: function (value) {
            quiz = value;
        },
        calculScoreTotal: function () {
            var scoreTotalTmp = 0;
            var scoreMaxTmp = 0;
            allResponses = [];
            for (var i = 0; i < quiz.questions.length ; i++) {
                scoreMaxTmp = scoreMaxTmp + Number(this.calculScoreMaxQuestion(quiz.questions[i]));
                for(var j = 0; j < quiz.questions[i].responses.length; j++)
                {
                    var response = quiz.questions[i].responses[j];
                    if (response.chosen)
                    {                       
                        scoreTotalTmp = scoreTotalTmp + (Number(response.score == undefined ? 0 : response.score) == NaN ? 0 : Number(response.score));
                    }                    
                    allResponses.push(response);                    
                }
            }
            scoreTotal = scoreTotalTmp;
            scoreMax = scoreMaxTmp;
            return scoreTotalTmp;
        },
        calculScoreMaxQuestion : function(question){
            var numberOfResponses = question.responses.length;
            var numberOfMaxresponses = question.numberOfPossibleAnswers;
            var responsesScoreArray = question.responses.map(function(s){ return s.score });
            //sort score of the responses
            responsesScoreArray.sort(function(a, b){return b-a});
            var result = responsesScoreArray.slice(0, numberOfMaxresponses);
            return result.reduce(function (a, b) { return a + b; }, 0);
        },
        getScoreTotal: function () {
            return scoreTotal;
        },
        getScoreMax: function () {
            return scoreMax;
        },
        getScoreBySkills: function () {
            if (allResponses.length == 0)
                return [];

            var skillScores = [];
            for (var i = 0; i < allResponses.length ; i++)
            {
                var skillScore = {};
                if (allResponses[i].chosen)
                    skillScore = { skill: allResponses[i].skill, score: Number(allResponses[i].score), scoreNormed: Number(allResponses[i].score) / Number(allResponses[i].score), max: Number(allResponses[i].score) };
                else
                    skillScore = { skill: allResponses[i].skill, score: Number(0), scoreNormed: Number(0), max: Number(allResponses[i].score) };

                // if liste of skill scores is empty we can directly add the new skillscore to the list.
                if (skillScores.length == 0) {
                    skillScores.push(skillScore);
                }                
                else {
                    // if liste of skill scores is NOT empty we verify if the skill is already in the liste.
                    var index = skillScores.map(function (a) { return a.skill }).indexOf(allResponses[i].skill);
                    if (index < 0) {
                        // if NOT we add directly this skill and the score
                        skillScores.push(skillScore);
                    }
                    else
                    {
                        // if YES we sum the score to the existing skill score object
                        skillScores[index].score = skillScores[index].score + Number(skillScore.score);
                        skillScores[index].max = skillScores[index].max + Number(skillScore.max);
                        skillScores[index].scoreNormed = skillScores[index].score / skillScores[index].max;
                    }
                }
            }
            return skillScores;            
        }    
    };
   
}

function SkillMapper() {
    var skillScoresByQuizs = [];
}

// this service is for manage Languages
function languageManager($translate, $localStorage) {
    return {
        setLanguageWithBrowserDefaultOrStoredValue: function () {
            if ($localStorage.language == null || $localStorage.language == undefined || $localStorage.language == "") {
                var lang = navigator.languages ? navigator.languages[0] : (navigator.language || navigator.userLanguage);
                $localStorage.language = lang;
                $translate.use(lang);
            }
            else
            {
                $translate.use($localStorage.language);
            }
        }
    };
}

/*/
SECURITY
/*/
function authInterceptor($rootScope, $localStorage) {
    return {
        request: function (config) {           
            config.headers = config.headers || {};
            if ($localStorage.Authorization) {
                config.headers.Authorization = 'Bearer ' + $localStorage.Authorization.access_token;
            }
            return config;
        }
    };
}

angular
    .module('coduco')
    .service('SearchTextService', SearchTextService)
    .service('UserManager', UserManager)
    .service('ObjectManager', ObjectManager)
    .service('QuizEngine', QuizEngine)
    .service('languageManager', languageManager)
    .factory('authInterceptor', authInterceptor)
    .config(function ($httpProvider) {
        $httpProvider.interceptors.push('authInterceptor');
    });