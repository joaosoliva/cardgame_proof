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
                    "Prepare: a próxima votação de conexão deste jogador precisa de unanimidade.",
                    ScienceActionEffectType.PeerReview,
                    ScienceActionTimingType.Prepared,
                    "Ação preparada. Jogue durante seu turno; ela fica ativa até sua próxima conexão. Quando você colocar uma carta de personagem, a votação dessa conexão exigirá unanimidade."),
                new ScienceActionCardData(
                    "action_citation_needed",
                    "Citação Necessária",
                    "Prepare: sua próxima conexão aceita destaca o bônus de guia/fato.",
                    ScienceActionEffectType.CitationNeeded,
                    ScienceActionTimingType.Prepared,
                    "Ação preparada. Jogue durante seu turno; ela fica ativa até sua próxima conexão. Se a conexão for aceita, o painel de pontuação lembrará o grupo de considerar +1 por uso de fato específico ou guia."),
                new ScienceActionCardData(
                    "action_interdisciplinary_leap",
                    "Salto Interdisciplinar",
                    "Prepare: sua próxima conexão pode defender uma ligação ousada entre áreas.",
                    ScienceActionEffectType.InterdisciplinaryLeap,
                    ScienceActionTimingType.Prepared,
                    "Ação preparada. Jogue durante seu turno; ela fica ativa até sua próxima conexão. Use-a para sinalizar que você tentará explicar uma ligação criativa entre áreas diferentes; o consenso do grupo continua decidindo.")
            };
        }
    }
}
