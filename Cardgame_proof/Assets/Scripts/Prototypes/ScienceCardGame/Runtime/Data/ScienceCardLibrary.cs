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
                    ScienceFactCategory.PhysicalSciences,
                    ScienceFactCategory.LifeSciences,
                    "Marie Curie pesquisou elementos radioativos e foi a primeira pessoa a receber dois Prêmios Nobel em áreas científicas diferentes."),
                new ScienceCharacterCardData(
                    "character_charles_darwin",
                    "Charles Darwin",
                    "Biologia",
                    "Propôs uma explicação para a diversidade da vida baseada em seleção natural.",
                    ScienceFactCategory.LifeSciences,
                    ScienceFactCategory.SocietyAndEducation,
                    "Charles Darwin reuniu observações de viagens, fósseis e seres vivos para formular a teoria da evolução por seleção natural."),
                new ScienceCharacterCardData(
                    "character_ada_lovelace",
                    "Ada Lovelace",
                    "Matemática e Computação",
                    "Antecipou ideias de algoritmos e computação simbólica.",
                    ScienceFactCategory.MathAndComputation,
                    ScienceFactCategory.TechnologyAndInvention,
                    "Ada Lovelace escreveu notas sobre a Máquina Analítica que hoje são associadas aos primeiros conceitos de programação."),
                new ScienceCharacterCardData(
                    "character_ibn_al_haytham",
                    "Ibn al-Haytham",
                    "Óptica",
                    "Relacionou luz, visão, observação e teste experimental.",
                    ScienceFactCategory.PhysicalSciences,
                    ScienceFactCategory.MathAndComputation,
                    "Ibn al-Haytham estudou a luz e a visão, valorizando evidências e testes como parte da investigação científica."),
                new ScienceCharacterCardData(
                    "character_rosalind_franklin",
                    "Rosalind Franklin",
                    "Química e Biologia Molecular",
                    "Produziu imagens essenciais para compreender a estrutura do DNA.",
                    ScienceFactCategory.TechnologyAndInvention,
                    ScienceFactCategory.LifeSciences,
                    "Rosalind Franklin usou cristalografia de raios X para estudar moléculas biológicas, incluindo o DNA."),
                new ScienceCharacterCardData(
                    "character_katherine_johnson",
                    "Katherine Johnson",
                    "Matemática e Astronáutica",
                    "Realizou cálculos fundamentais para missões espaciais.",
                    ScienceFactCategory.MathAndComputation,
                    ScienceFactCategory.PhysicalSciences,
                    "Katherine Johnson calculou trajetórias orbitais e contribuiu para missões espaciais tripuladas da NASA."),
                new ScienceCharacterCardData(
                    "character_rachel_carson",
                    "Rachel Carson",
                    "Biologia Marinha e Ecologia",
                    "Conectou ciência ambiental, sociedade e responsabilidade pública.",
                    ScienceFactCategory.LifeSciences,
                    ScienceFactCategory.SocietyAndEducation,
                    "Rachel Carson escreveu sobre impactos ambientais de pesticidas e influenciou debates modernos sobre conservação."),
                new ScienceCharacterCardData(
                    "character_nise_da_silveira",
                    "Nise da Silveira",
                    "Psiquiatria e Saúde Mental",
                    "Defendeu cuidado humanizado e expressão artística em saúde mental.",
                    ScienceFactCategory.LifeSciences,
                    ScienceFactCategory.SocietyAndEducation,
                    "Nise da Silveira transformou práticas psiquiátricas ao valorizar vínculo, criatividade e respeito aos pacientes."),
                new ScienceCharacterCardData(
                    "character_grace_hopper",
                    "Grace Hopper",
                    "Computação e Linguagens de Programação",
                    "Ajudou a tornar computadores mais acessíveis por meio de linguagens e ferramentas de programação.",
                    ScienceFactCategory.MathAndComputation,
                    ScienceFactCategory.TechnologyAndInvention,
                    "Grace Hopper trabalhou em compiladores e influenciou linguagens de programação que aproximaram pessoas e máquinas."),
                new ScienceCharacterCardData(
                    "character_nikola_tesla",
                    "Nikola Tesla",
                    "Física e Engenharia Elétrica",
                    "Contribuiu para sistemas elétricos e invenções ligadas à corrente alternada.",
                    ScienceFactCategory.PhysicalSciences,
                    ScienceFactCategory.TechnologyAndInvention,
                    "Nikola Tesla desenvolveu ideias e dispositivos ligados à eletricidade, motores e transmissão de energia em corrente alternada."),
                new ScienceActionCardData(
                    "action_peer_review",
                    "Revisão por Pares",
                    "Prepare: se sua próxima conexão for recusada, você pode pedir uma segunda votação.",
                    ScienceActionEffectType.PeerReview,
                    ScienceActionTimingType.Prepared,
                    "Prepare esta ação. Se sua próxima conexão for recusada, você pode reformular a explicação e pedir uma segunda votação."),
                new ScienceActionCardData(
                    "action_citation_needed",
                    "Citação Necessária",
                    "Abra uma bio; se usar um fato dela na próxima explicação aceita, ganhe +1.",
                    ScienceActionEffectType.CitationNeeded,
                    ScienceActionTimingType.Immediate,
                    "Abra a bio de uma carta. Se você usar um fato dessa bio na sua próxima explicação e o grupo aceitar, ganhe +1 ponto bônus."),
                new ScienceActionCardData(
                    "action_interdisciplinary_leap",
                    "Salto Interdisciplinar",
                    "Prepare: na próxima conexão entre cores diferentes aceita, ganhe +1 bônus.",
                    ScienceActionEffectType.InterdisciplinaryLeap,
                    ScienceActionTimingType.Prepared,
                    "Prepare esta ação. Na sua próxima conexão entre cores diferentes, se o grupo aceitar sua explicação, ganhe +1 ponto bônus.")
            };
        }
    }
}
