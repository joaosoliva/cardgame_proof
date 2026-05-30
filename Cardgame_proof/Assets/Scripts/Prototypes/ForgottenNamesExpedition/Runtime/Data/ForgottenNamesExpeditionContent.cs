using System.Collections.Generic;

namespace CardgameProof.Prototypes.ForgottenNamesExpedition.Runtime.Data
{
    public enum ForgottenNamesCardType
    {
        Question,
        Scientist,
        Challenge,
        Final
    }

    public readonly struct ForgottenNamesPremise
    {
        public ForgottenNamesPremise(string id, string title, string description, string tableReminder, string questionBridge, string scientistBridge, string challengeBridge, string finalBridge, string[] answerHints)
        {
            Id = id;
            Title = title;
            Description = description;
            TableReminder = tableReminder;
            QuestionBridge = questionBridge;
            ScientistBridge = scientistBridge;
            ChallengeBridge = challengeBridge;
            FinalBridge = finalBridge;
            AnswerHints = answerHints;
        }

        public string Id { get; }
        public string Title { get; }
        public string Description { get; }
        public string TableReminder { get; }
        public string QuestionBridge { get; }
        public string ScientistBridge { get; }
        public string ChallengeBridge { get; }
        public string FinalBridge { get; }
        public string[] AnswerHints { get; }
    }

    public readonly struct ForgottenNamesRole
    {
        public ForgottenNamesRole(string title, string shortDescription)
        {
            Title = title;
            ShortDescription = shortDescription;
        }

        public string Title { get; }
        public string ShortDescription { get; }
    }

    public readonly struct ForgottenNamesQuestion
    {
        public ForgottenNamesQuestion(string title, string body, string question, string helper)
        {
            Title = title;
            Body = body;
            Question = question;
            Helper = helper;
        }

        public string Title { get; }
        public string Body { get; }
        public string Question { get; }
        public string Helper { get; }
    }

    public readonly struct ForgottenNamesScientist
    {
        public ForgottenNamesScientist(string name, string field, string shortContribution, string humanHook, string[] tags, string encounterPrompt, string archivePrompt, string fieldGuideBio)
        {
            Name = name;
            Field = field;
            ShortContribution = shortContribution;
            HumanHook = humanHook;
            Tags = tags;
            EncounterPrompt = encounterPrompt;
            ArchivePrompt = archivePrompt;
            FieldGuideBio = fieldGuideBio;
        }

        public string Name { get; }
        public string Field { get; }
        public string ShortContribution { get; }
        public string HumanHook { get; }
        public string[] Tags { get; }
        public string EncounterPrompt { get; }
        public string ArchivePrompt { get; }
        public string FieldGuideBio { get; }
    }

    public readonly struct ForgottenNamesChallenge
    {
        public ForgottenNamesChallenge(string title, string situation, string[] recommendedTags, string matchedPrompt, string unmatchedPrompt)
        {
            Title = title;
            Situation = situation;
            RecommendedTags = recommendedTags;
            MatchedPrompt = matchedPrompt;
            UnmatchedPrompt = unmatchedPrompt;
        }

        public string Title { get; }
        public string Situation { get; }
        public string[] RecommendedTags { get; }
        public string MatchedPrompt { get; }
        public string UnmatchedPrompt { get; }
    }

    public readonly struct ForgottenNamesFinalCard
    {
        public ForgottenNamesFinalCard(string title, string body, string finalPrompt, string groupSentence)
        {
            Title = title;
            Body = body;
            FinalPrompt = finalPrompt;
            GroupSentence = groupSentence;
        }

        public string Title { get; }
        public string Body { get; }
        public string FinalPrompt { get; }
        public string GroupSentence { get; }
    }

    public readonly struct ForgottenNamesDeckCard
    {
        public ForgottenNamesDeckCard(ForgottenNamesCardType type, int contentIndex)
        {
            Type = type;
            ContentIndex = contentIndex;
        }

        public ForgottenNamesCardType Type { get; }
        public int ContentIndex { get; }
    }

    public static class ForgottenNamesExpeditionContent
    {
        public static readonly IReadOnlyList<ForgottenNamesPremise> Premises = new List<ForgottenNamesPremise>
        {
            new ForgottenNamesPremise(
                "lost_archive",
                "O Arquivo Perdido",
                "Vocês encontraram uma caixa de anotações antigas em um arquivo esquecido. Dentro dela, há nomes, ideias e descobertas que quase desapareceram.",
                "A expedição está reconstruindo uma memória quase perdida.",
                "No arquivo, uma nova pista aparece.",
                "Entre os registros do arquivo, este nome pede atenção.",
                "O arquivo oferece uma pista, mas ela não se explica sozinha.",
                "O arquivo foi aberto. Agora é preciso decidir o que não volta para o esquecimento.",
                new[] { "documentos", "nomes", "datas", "páginas faltando", "objetos", "caligrafia", "caixas", "salas esquecidas" }),
            new ForgottenNamesPremise(
                "opening_exhibition",
                "A Exposição que Abre Hoje",
                "Um museu precisa montar uma pequena exposição até o fim do dia. O desafio é decidir quais nomes e contribuições não podem ficar de fora.",
                "A expedição está escolhendo como uma descoberta será apresentada ao público.",
                "Na preparação da exposição, uma escolha precisa ser feita.",
                "Entre as possíveis peças da exposição, esta contribuição pede atenção.",
                "A exposição precisa transformar conhecimento em algo que o público consiga perceber.",
                "A exposição está pronta para receber visitantes. Agora é preciso decidir que memória ela vai carregar.",
                new[] { "visitantes", "objetos", "legendas", "salas", "imagens", "primeira impressão", "vitrine", "explicação simples" }),
            new ForgottenNamesPremise(
                "unfunded_question",
                "A Pergunta que Ninguém Financiou",
                "Uma pesquisa importante nunca recebeu apoio suficiente. Agora, o grupo precisa reconstruir por que essa pergunta importava.",
                "A expedição está defendendo o valor de uma pergunta que quase ninguém quis escutar.",
                "Na pesquisa interrompida, uma dúvida volta a aparecer.",
                "Enquanto seguem a pergunta esquecida, o grupo encontra uma contribuição que pode mudar o caminho.",
                "A pergunta continua difícil, mas uma nova tentativa pode revelar seu valor.",
                "A pergunta finalmente encontrou escuta. Agora é preciso decidir o que fazer com essa resposta.",
                new[] { "hipótese", "tentativa", "anotação", "falta de recursos", "defesa", "curiosidade", "persistência", "valor escondido" })
        };

        public static readonly IReadOnlyList<ForgottenNamesRole> Roles = new List<ForgottenNamesRole>
        {
            new ForgottenNamesRole("Estudante", "Alguém que ainda está aprendendo a olhar para a ciência com curiosidade."),
            new ForgottenNamesRole("Arquivista", "Alguém que protege documentos, rastros e memórias."),
            new ForgottenNamesRole("Jornalista", "Alguém que transforma descobertas em histórias públicas."),
            new ForgottenNamesRole("Professor(a)", "Alguém que ajuda outras pessoas a entenderem ideias difíceis."),
            new ForgottenNamesRole("Curador(a)", "Alguém que escolhe como uma descoberta será apresentada."),
            new ForgottenNamesRole("Inventor(a)", "Alguém que imagina novos usos para conhecimentos antigos.")
        };

        public static readonly IReadOnlyList<ForgottenNamesQuestion> Questions = new List<ForgottenNamesQuestion>
        {
            new ForgottenNamesQuestion(
                "O primeiro detalhe",
                "A premissa coloca muitas pistas, objetos ou ideias diante do grupo, mas uma delas chama atenção primeiro.",
                "Que detalhe seu personagem escolhe observar antes dos outros, e por que ele parece importante?",
                "Use a carta de premissa como cena. Uma frase já é suficiente."),
            new ForgottenNamesQuestion(
                "O que ficou de fora",
                "Toda memória preserva algumas coisas e deixa outras quase invisíveis.",
                "O que seu personagem acha que está faltando nesta versão da história?",
                "Pense em autoria, contexto, cuidado, trabalho invisível ou consequências."),
            new ForgottenNamesQuestion(
                "A escolha de atenção",
                "O grupo não consegue dar o mesmo foco para tudo ao mesmo tempo.",
                "O que seu personagem acredita que merece atenção agora, mesmo que outras coisas precisem esperar?",
                "Não existe resposta certa. Escolha algo que ajude esta expedição a tomar forma."),
            new ForgottenNamesQuestion(
                "A contribuição quase invisível",
                "A expedição encontra algo que poderia facilmente ser tratado como detalhe menor ou nota de rodapé.",
                "Por que seu personagem acha que essa contribuição importa?",
                "Responda pensando no que costuma ser esquecido quando contamos histórias de ciência."),
            new ForgottenNamesQuestion(
                "Explicar para alguém de fora",
                "Em algum momento, alguém fora da expedição precisará entender por que esta descoberta merece cuidado.",
                "Como seu personagem explicaria a importância disso em uma frase?",
                "Tente falar como se explicasse para uma pessoa curiosa, mas sem conhecimento prévio."),
            new ForgottenNamesQuestion(
                "O cuidado necessário",
                "Nem tudo na expedição deve ser tratado com pressa. Algumas coisas pedem cuidado.",
                "O que seu personagem decide tratar com mais cuidado, e por quê?",
                "Pode ser um nome, objeto, ideia, pista, cálculo, relato ou contribuição."),
            new ForgottenNamesQuestion(
                "A dúvida que quase interrompe tudo",
                "Por um momento, o grupo se pergunta se esse trabalho será compreendido ou lembrado por alguém.",
                "O que faz seu personagem continuar mesmo assim?",
                "A resposta pode ser pequena: uma pessoa, uma pista, uma frase ou uma sensação."),
            new ForgottenNamesQuestion(
                "Um nome que muda o caminho",
                "Às vezes, encontrar um nome muda a forma como enxergamos todo o resto.",
                "Que tipo de nome seu personagem espera encontrar nesta expedição?",
                "Pense em alguém que observou, calculou, cuidou, ensinou, coletou, testou ou insistiu."),
            new ForgottenNamesQuestion(
                "Uma nova forma de lembrar",
                "A expedição começa a mudar o que o grupo entende por memória.",
                "Depois do que encontrou até agora, o que seu personagem acha que lembrar deveria significar?",
                "Você pode responder com uma frase simples."),
            new ForgottenNamesQuestion(
                "O próximo passo",
                "A premissa não se resolve sozinha. O grupo precisa escolher como continuar.",
                "Qual é o próximo pequeno passo que seu personagem propõe para a expedição?",
                "Pode ser observar melhor, comparar pistas, ouvir alguém, explicar uma ideia ou registrar um nome.")
        };

        public static readonly IReadOnlyList<ForgottenNamesScientist> Scientists = new List<ForgottenNamesScientist>
        {
            new ForgottenNamesScientist(
                "Mary Anning",
                "Paleontologia",
                "Encontrou e estudou fósseis que ajudaram a transformar a compreensão sobre extinção e vida pré-histórica.",
                "Seu trabalho foi essencial, mas gênero e classe social limitaram seu reconhecimento em vida.",
                new[] { "Evidências", "Fósseis", "Paciência", "Mundos Antigos" },
                "Mary Anning pode ajudar a expedição a ler pistas escondidas em pedras, ruínas e vestígios. Ela entra na Party ou seu nome é registrado no Archive?",
                "Que detalhe sobre Mary Anning o grupo promete não esquecer?",
                "Mary Anning foi uma importante coletora e estudiosa de fósseis no século XIX. Suas descobertas ajudaram a ciência a pensar sobre extinção e sobre a longa história da vida na Terra."),
            new ForgottenNamesScientist(
                "Wang Zhenyi",
                "Astronomia, matemática e poesia",
                "Explicou eclipses e escreveu sobre astronomia de forma acessível.",
                "Estudou e produziu conhecimento em uma época em que poucas mulheres tinham espaço para isso.",
                new[] { "Céu", "Ciclos", "Padrões", "Explicação" },
                "Wang Zhenyi pode ajudar a expedição a entender padrões no céu, ciclos e explicações complexas. Ela entra na Party ou seu nome é registrado no Archive?",
                "Que ideia de Wang Zhenyi o grupo quer manter viva?",
                "Wang Zhenyi foi uma estudiosa chinesa que escreveu sobre astronomia, matemática e poesia. Ela explicou fenômenos como eclipses de maneira clara e acessível."),
            new ForgottenNamesScientist(
                "Alice Ball",
                "Química",
                "Desenvolveu um tratamento importante para a hanseníase no início do século XX.",
                "Sua contribuição foi por muito tempo pouco creditada.",
                new[] { "Química", "Cuidado", "Tratamento", "Justiça" },
                "Alice Ball pode ajudar a expedição quando conhecimento científico precisa virar cuidado concreto. Ela entra na Party ou seu nome é registrado no Archive?",
                "Como o grupo registra a importância de Alice Ball?",
                "Alice Ball foi uma química que desenvolveu um método usado no tratamento da hanseníase. Seu trabalho teve impacto real na vida de pacientes, mas seu nome demorou a receber reconhecimento."),
            new ForgottenNamesScientist(
                "Chien-Shiung Wu",
                "Física experimental",
                "Realizou experimentos fundamentais sobre partículas e simetria na física.",
                "Seu trabalho foi crucial para descobertas reconhecidas internacionalmente, mas nem sempre recebeu o mesmo destaque.",
                new[] { "Experimento", "Precisão", "Física", "Evidências" },
                "Chien-Shiung Wu pode ajudar a expedição a testar uma hipótese com precisão. Ela entra na Party ou seu nome é registrado no Archive?",
                "Que qualidade do trabalho de Chien-Shiung Wu o grupo quer lembrar?",
                "Chien-Shiung Wu foi uma física experimental conhecida por trabalhos decisivos em física nuclear. Sua precisão ajudou a transformar debates teóricos em evidências concretas."),
            new ForgottenNamesScientist(
                "Katherine Johnson",
                "Matemática",
                "Fez cálculos fundamentais de trajetórias para missões espaciais.",
                "Seu trabalho ajudou missões históricas, mas por muito tempo ficou menos visível ao público.",
                new[] { "Cálculo", "Trajetória", "Espaço", "Confiança" },
                "Katherine Johnson pode ajudar a expedição a calcular caminhos difíceis e tomar decisões com precisão. Ela entra na Party ou seu nome é registrado no Archive?",
                "Que trajetória Katherine Johnson ajuda o grupo a lembrar?",
                "Katherine Johnson foi uma matemática da NASA. Seus cálculos de trajetória foram fundamentais para missões espaciais, mostrando como precisão matemática pode sustentar grandes jornadas."),
            new ForgottenNamesScientist(
                "Bertha Lutz",
                "Biologia, educação e direitos",
                "Atuou na ciência, na educação e na defesa da participação das mulheres na vida pública.",
                "Seu trabalho conecta conhecimento científico, participação social e memória histórica.",
                new[] { "Comunidade", "Educação", "Natureza", "Direitos" },
                "Bertha Lutz pode ajudar a expedição a conectar conhecimento, educação e participação pública. Ela entra na Party ou seu nome é registrado no Archive?",
                "Que ponte entre ciência e sociedade o grupo registra ao lembrar Bertha Lutz?",
                "Bertha Lutz foi bióloga, educadora e uma figura importante na defesa dos direitos das mulheres no Brasil. Sua trajetória mostra como ciência e participação social podem caminhar juntas.")
        };

        public static readonly IReadOnlyList<ForgottenNamesChallenge> Challenges = new List<ForgottenNamesChallenge>
        {
            new ForgottenNamesChallenge("O padrão escondido", "O grupo encontra uma repetição nas anotações que ninguém tinha percebido.", new[] { "Padrões", "Cálculo", "Evidências" }, "Como essa figura ajuda o grupo a perceber o padrão?", "Essa não era a especialidade principal da figura escolhida. Que esforço extra o grupo faz para entender o padrão mesmo assim?"),
            new ForgottenNamesChallenge("A explicação precisa ser clara", "O grupo precisa explicar uma descoberta para pessoas que nunca ouviram falar sobre o tema.", new[] { "Explicação", "Educação", "Comunidade" }, "Como essa figura ajuda o grupo a tornar a ideia mais clara?", "O grupo consegue explicar, mas precisa improvisar. O que ficou mais difícil de comunicar?"),
            new ForgottenNamesChallenge("A prova quase passa despercebida", "Uma pista importante parece pequena demais para convencer os outros.", new[] { "Evidências", "Experimento", "Precisão", "Paciência" }, "Como essa figura ajuda o grupo a mostrar que a pista importa?", "Sem a especialidade ideal, como o grupo decide defender essa pista?")
        };

        public static readonly ForgottenNamesFinalCard FinalCard = new ForgottenNamesFinalCard(
            "O que não deixamos desaparecer",
            "A expedição termina quando esta carta aparecer.",
            "Que nome, ideia ou contribuição seu personagem escolhe preservar — e como?",
            "Nós lembramos ______ porque ______.");

        public static readonly IReadOnlyList<string> TutorialRules = new List<string>
        {
            "Leia cada carta em voz alta.",
            "Responda com uma frase ou com mais detalhes. As duas formas são válidas.",
            "Cientistas na Party ajudam agora. Cientistas no Archive serão lembrados depois.",
            "No final, o grupo decide o que não quer deixar desaparecer."
        };

        public static string JoinTags(IEnumerable<string> tags)
        {
            return tags == null ? string.Empty : string.Join(" • ", tags);
        }

        public static bool HasMatchingTag(ForgottenNamesScientist scientist, ForgottenNamesChallenge challenge)
        {
            if (scientist.Tags == null || challenge.RecommendedTags == null) return false;
            foreach (string scientistTag in scientist.Tags)
            {
                foreach (string recommendedTag in challenge.RecommendedTags)
                {
                    if (scientistTag == recommendedTag) return true;
                }
            }

            return false;
        }
    }
}
