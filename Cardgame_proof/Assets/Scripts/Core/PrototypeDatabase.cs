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
            new ArchiveCardData
            {
                Id = "archive_area",
                CardType = CardType.Archive,
                Title = "Arquivo de Área",
                Description = "Revela uma pista sobre o campo de atuação da personagem.",
                ClueCategory = ClueCategory.Area,
                Prompt = "A área principal desta personagem é científica, tecnológica ou de cuidado em saúde?"
            },
            new ArchiveCardData
            {
                Id = "archive_era",
                CardType = CardType.Archive,
                Title = "Arquivo de Era",
                Description = "Revela uma pista temporal sobre o período histórico da personagem.",
                ClueCategory = ClueCategory.Era,
                Prompt = "Em que período histórico essa personagem se destacou?"
            },
            new ArchiveCardData
            {
                Id = "archive_region",
                CardType = CardType.Archive,
                Title = "Arquivo de Região",
                Description = "Revela uma pista geográfica sobre a trajetória da personagem.",
                ClueCategory = ClueCategory.Region,
                Prompt = "Qual região está mais associada à vida e ao trabalho da personagem?"
            }
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
                    CharactersPerPlayer = 2,
                    ArchiveCardsPerPlayer = 2,
                    ResearchTokensPerPlayer = 1,
                    BoardSize = new Vector2Int(4, 4),
                    ObjectiveIdentifications = 1
                },
                ["10min"] = new GameModeConfig
                {
                    Id = "10min",
                    DisplayName = "Partida Estendida (10 min)",
                    DurationMinutes = 10,
                    TotalCharacters = 6,
                    CharactersPerPlayer = 3,
                    ArchiveCardsPerPlayer = 3,
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
