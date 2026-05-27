using System.Collections.Generic;
using UnityEngine;

namespace CardgameProof.Core
{
    public static class PrototypeDatabase
    {
        public static readonly IReadOnlyList<CharacterData> Characters = new List<CharacterData>
        {
            new CharacterData
            {
                Id = "hypatia",
                DisplayName = "Hipátia de Alexandria",
                Area = "Matemática e Filosofia",
                Era = "Antiguidade Tardia (séculos IV-V)",
                Region = "Alexandria, Egito",
                Contribution = "Ensinou matemática e astronomia e preservou tradições científicas clássicas.",
                ContextOrLegacy = "Tornou-se símbolo da defesa do conhecimento e da liberdade intelectual.",
                GuidebookBioPtBr = "Hipátia foi uma professora e pensadora de Alexandria. Sua atuação em matemática, astronomia e filosofia inspirou gerações e representa a importância de proteger o saber em tempos de conflito."
            },
            new CharacterData
            {
                Id = "katherine_johnson",
                DisplayName = "Katherine Johnson",
                Area = "Matemática Aplicada",
                Era = "Século XX",
                Region = "Estados Unidos",
                Contribution = "Calculou trajetórias para missões espaciais da NASA, incluindo órbitas e reentrada.",
                ContextOrLegacy = "Seu trabalho foi essencial para a corrida espacial e para a valorização de mulheres negras na ciência.",
                GuidebookBioPtBr = "Katherine Johnson foi uma matemática brilhante que ajudou a levar astronautas ao espaço com segurança. Seu legado mostra como precisão científica e coragem podem mudar a história."
            },
            new CharacterData
            {
                Id = "chien_shiung_wu",
                DisplayName = "Chien-Shiung Wu",
                Area = "Física Experimental",
                Era = "Século XX",
                Region = "China e Estados Unidos",
                Contribution = "Conduziu o experimento que demonstrou a violação da paridade em interações fracas.",
                ContextOrLegacy = "É referência em rigor experimental e na contribuição de mulheres para a física moderna.",
                GuidebookBioPtBr = "Chien-Shiung Wu foi uma física experimental reconhecida mundialmente. Seus experimentos transformaram teorias fundamentais e reforçam o valor da investigação cuidadosa."
            },
            new CharacterData
            {
                Id = "ibn_al_haytham",
                DisplayName = "Ibn al-Haytham",
                Area = "Óptica e Método Científico",
                Era = "Idade de Ouro Islâmica (séculos X-XI)",
                Region = "Basra e Cairo",
                Contribution = "Sistematizou estudos de óptica e enfatizou observação e teste em investigações científicas.",
                ContextOrLegacy = "Sua abordagem influenciou o desenvolvimento do método científico em diversas áreas.",
                GuidebookBioPtBr = "Ibn al-Haytham investigou como a luz se comporta e defendeu testar hipóteses com evidências. Seu trabalho é base para práticas científicas até hoje."
            },
            new CharacterData
            {
                Id = "nise_da_silveira",
                DisplayName = "Nise da Silveira",
                Area = "Psiquiatria e Saúde Mental",
                Era = "Século XX",
                Region = "Brasil",
                Contribution = "Criou abordagens terapêuticas humanizadas com arte e expressão para pacientes psiquiátricos.",
                ContextOrLegacy = "Revolucionou práticas em saúde mental ao priorizar dignidade, vínculo e cuidado não violento.",
                GuidebookBioPtBr = "Nise da Silveira foi uma médica brasileira que transformou a psiquiatria ao defender tratamentos humanos e criativos. Seu legado inspira cuidado com empatia e respeito."
            },
            new CharacterData
            {
                Id = "ada_lovelace",
                DisplayName = "Ada Lovelace",
                Area = "Matemática e Computação",
                Era = "Século XIX",
                Region = "Reino Unido",
                Contribution = "Escreveu notas sobre a Máquina Analítica com ideias de algoritmos e processamento simbólico.",
                ContextOrLegacy = "É reconhecida como pioneira da programação e da visão de computadores além de cálculos numéricos.",
                GuidebookBioPtBr = "Ada Lovelace imaginou como máquinas poderiam seguir instruções para resolver problemas complexos. Sua visão antecipou conceitos centrais da computação moderna."
            }
        };

        public static readonly IReadOnlyList<ArchiveCardData> ArchiveCards = new List<ArchiveCardData>
        {
            new ArchiveCardData { Id = "archive_cross_ref", CardType = CardType.Archive, Title = "Referência Cruzada", Description = "Escolha um Dossiê encontrado. Revele uma pista extra dele.", ClueCategory = ClueCategory.Area, Prompt = "Revela uma pista extra." },
            new ArchiveCardData { Id = "archive_fragment", CardType = CardType.Archive, Title = "Fragmento de Documento", Description = "Revele uma pista aleatória de um Dossiê encontrado.", ClueCategory = ClueCategory.Contribution, Prompt = "Revela pista aleatória." },
            new ArchiveCardData { Id = "archive_index", CardType = CardType.Archive, Title = "Índice do Arquivo", Description = "Escolha um Dossiê encontrado. Revele Área ou Época.", ClueCategory = ClueCategory.Era, Prompt = "Revela área/época." },
            new ArchiveCardData { Id = "archive_footnote", CardType = CardType.Archive, Title = "Nota de Rodapé", Description = "Escolha uma pista já revelada. Mostre dica curta de interpretação.", ClueCategory = ClueCategory.ContextLegacy, Prompt = "Dica de interpretação." },
            new ArchiveCardData { Id = "archive_targeted", CardType = CardType.Archive, Title = "Pista Direcionada", Description = "Escolha um Dossiê encontrado e uma categoria de pista. Revele essa pista.", ClueCategory = ClueCategory.Region, Prompt = "Revela categoria escolhida." },
            new ArchiveCardData { Id = "archive_organized", CardType = CardType.Archive, Title = "Arquivo Organizado", Description = "Investigue outra carta imediatamente.", ClueCategory = ClueCategory.Area, Prompt = "Ação extra." }
        };

                public static readonly IReadOnlyDictionary<string, GameModeConfig> GameModes =
            new Dictionary<string, GameModeConfig>
            {
                ["5min"] = new GameModeConfig
                {
                    Id = "5min",
                    DisplayName = "Partida Rápida (5 min)",
                    DurationMinutes = 5,
                    TotalCharacters = 4,
                    CharactersPerPlayer = 3,
                    ArchiveCardsPerPlayer = 3,
                    ResearchTokensPerPlayer = 1,
                    BoardSize = new Vector2Int(3, 3),
                    ObjectiveIdentifications = 1
                },
                ["10min"] = new GameModeConfig
                {
                    Id = "10min",
                    DisplayName = "Partida Estendida (10 min)",
                    DurationMinutes = 10,
                    TotalCharacters = 8,
                    CharactersPerPlayer = 4,
                    ArchiveCardsPerPlayer = 6,
                    ResearchTokensPerPlayer = 2,
                    BoardSize = new Vector2Int(4, 4),
                    ObjectiveIdentifications = 2
                }
            };

        public static GameModeConfig GetMode(string modeId)
        {
            return GameModes.TryGetValue(modeId, out GameModeConfig mode) ? mode : null;
        }
    }
}
