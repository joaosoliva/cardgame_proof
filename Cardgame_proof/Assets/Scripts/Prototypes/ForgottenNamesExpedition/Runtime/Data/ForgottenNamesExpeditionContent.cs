using System.Collections.Generic;
using UnityEngine;

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
        public ForgottenNamesPremise(string title, string description)
        {
            Title = title;
            Description = description;
        }

        public string Title { get; }
        public string Description { get; }
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
            new ForgottenNamesPremise("O Arquivo Perdido", "Vocês encontraram uma caixa de anotações antigas em um arquivo esquecido. Dentro dela, há nomes, ideias e descobertas que quase desapareceram."),
            new ForgottenNamesPremise("A Exposição que Abre Hoje", "Um museu precisa montar uma pequena exposição até o fim do dia. O desafio é decidir quais nomes e contribuições não podem ficar de fora."),
            new ForgottenNamesPremise("A Pergunta que Ninguém Financiou", "Uma pesquisa importante nunca recebeu apoio suficiente. Agora, o grupo precisa reconstruir por que essa pergunta importava.")
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
            new ForgottenNamesQuestion("Por que esta jornada importa?", "Antes de começar, cada expedição precisa de um motivo.", "O que fez seu personagem acreditar que essa busca valia a pena?", "Você pode responder com uma frase."),
            new ForgottenNamesQuestion("Um nome fora da página", "No arquivo, há sinais de que alguém importante ficou fora da versão mais conhecida da história.", "Que tipo de pessoa seu personagem acha que a história costuma esquecer?", "Pense em alguém que observa, cuida, calcula, ensina, coleta ou insiste."),
            new ForgottenNamesQuestion("Um conhecimento que você ignorava", "Nem todo conhecimento parece importante à primeira vista.", "Que tipo de conhecimento seu personagem costumava ignorar antes desta expedição?", "Exemplo: plantas, estrelas, números, relatos, mapas, instrumentos, trabalho de campo."),
            new ForgottenNamesQuestion("A primeira pista", "O grupo encontra uma anotação incompleta, mas promissora.", "Qual detalhe chama sua atenção e faz o grupo continuar?", "Pode ser uma palavra, desenho, fórmula, mapa, nome ou objeto."),
            new ForgottenNamesQuestion("O que mudou em você?", "A expedição já mostrou que conhecimento também depende de quem é lembrado.", "O que seu personagem começa a enxergar de outro jeito?", "Uma resposta simples é suficiente.")
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

        public static readonly IReadOnlyList<ForgottenNamesDeckCard> DemoDeck = new List<ForgottenNamesDeckCard>
        {
            new ForgottenNamesDeckCard(ForgottenNamesCardType.Question, 0),
            new ForgottenNamesDeckCard(ForgottenNamesCardType.Scientist, 0),
            new ForgottenNamesDeckCard(ForgottenNamesCardType.Question, 1),
            new ForgottenNamesDeckCard(ForgottenNamesCardType.Scientist, 1),
            new ForgottenNamesDeckCard(ForgottenNamesCardType.Challenge, 0),
            new ForgottenNamesDeckCard(ForgottenNamesCardType.Scientist, 2),
            new ForgottenNamesDeckCard(ForgottenNamesCardType.Challenge, 1),
            new ForgottenNamesDeckCard(ForgottenNamesCardType.Question, 4),
            new ForgottenNamesDeckCard(ForgottenNamesCardType.Final, 0)
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
