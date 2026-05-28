using System.Collections.Generic;

namespace CardgameProof.Prototypes.ScienceCardGame.Runtime.Data
{
    public static class ScienceCardLibrary
    {
        public static List<ScienceCardData> CreateDemoDeck()
        {
            return new List<ScienceCardData>
            {
                new ScienceCharacterCardData(
                    "character_marie_curie",
                    "Marie Curie",
                    "Química e Física",
                    "Investigou a radioatividade e abriu caminhos para aplicações médicas e científicas.",
                    ScienceFactCategory.Experimentation,
                    ScienceFactCategory.Health,
                    "Marie Curie pesquisou elementos radioativos e foi a primeira pessoa a receber dois Prêmios Nobel em áreas científicas diferentes."),
                new ScienceCharacterCardData(
                    "character_charles_darwin",
                    "Charles Darwin",
                    "Biologia",
                    "Propôs uma explicação para a diversidade da vida baseada em seleção natural.",
                    ScienceFactCategory.Observation,
                    ScienceFactCategory.Theory,
                    "Charles Darwin reuniu observações de viagens, fósseis e seres vivos para formular a teoria da evolução por seleção natural."),
                new ScienceCharacterCardData(
                    "character_ada_lovelace",
                    "Ada Lovelace",
                    "Matemática e Computação",
                    "Antecipou ideias de algoritmos e computação simbólica.",
                    ScienceFactCategory.Mathematics,
                    ScienceFactCategory.Technology,
                    "Ada Lovelace escreveu notas sobre a Máquina Analítica que hoje são associadas aos primeiros conceitos de programação."),
                new ScienceCharacterCardData(
                    "character_ibn_al_haytham",
                    "Ibn al-Haytham",
                    "Óptica",
                    "Relacionou luz, visão, observação e teste experimental.",
                    ScienceFactCategory.Experimentation,
                    ScienceFactCategory.Observation,
                    "Ibn al-Haytham estudou a luz e a visão, valorizando evidências e testes como parte da investigação científica."),
                new ScienceCharacterCardData(
                    "character_rosalind_franklin",
                    "Rosalind Franklin",
                    "Química e Biologia Molecular",
                    "Produziu imagens essenciais para compreender a estrutura do DNA.",
                    ScienceFactCategory.Technology,
                    ScienceFactCategory.Health,
                    "Rosalind Franklin usou cristalografia de raios X para estudar moléculas biológicas, incluindo o DNA."),
                new ScienceCharacterCardData(
                    "character_katherine_johnson",
                    "Katherine Johnson",
                    "Matemática e Astronáutica",
                    "Realizou cálculos fundamentais para missões espaciais.",
                    ScienceFactCategory.Mathematics,
                    ScienceFactCategory.Technology,
                    "Katherine Johnson calculou trajetórias orbitais e contribuiu para missões espaciais tripuladas da NASA."),
                new ScienceCharacterCardData(
                    "character_rachel_carson",
                    "Rachel Carson",
                    "Biologia Marinha e Ecologia",
                    "Conectou ciência ambiental, sociedade e responsabilidade pública.",
                    ScienceFactCategory.Environment,
                    ScienceFactCategory.Society,
                    "Rachel Carson escreveu sobre impactos ambientais de pesticidas e influenciou debates modernos sobre conservação."),
                new ScienceCharacterCardData(
                    "character_nise_da_silveira",
                    "Nise da Silveira",
                    "Psiquiatria e Saúde Mental",
                    "Defendeu cuidado humanizado e expressão artística em saúde mental.",
                    ScienceFactCategory.Health,
                    ScienceFactCategory.Society,
                    "Nise da Silveira transformou práticas psiquiátricas ao valorizar vínculo, criatividade e respeito aos pacientes."),
                new ScienceActionCardData(
                    "action_observar",
                    "Observação Cuidadosa",
                    "Compre uma carta e destaque uma categoria de fato em comum.",
                    ScienceActionEffectType.DrawCards,
                    "Compre 1 carta. Se ela compartilhar uma categoria com uma carta em mesa, anuncie a conexão."),
                new ScienceActionCardData(
                    "action_conexao",
                    "Conexão Científica",
                    "Crie uma ligação entre duas cartas de personagem.",
                    ScienceActionEffectType.CreateConnection,
                    "Escolha 2 personagens em jogo e explique uma conexão por área, método, impacto ou contexto."),
                new ScienceActionCardData(
                    "action_pesquisa",
                    "Pesquisa Rápida",
                    "Use uma mini biografia para apoiar uma conexão.",
                    ScienceActionEffectType.ResearchHint,
                    "Leia a mini biografia de uma carta e use essa informação para justificar uma conexão.")
            };
        }
    }
}
