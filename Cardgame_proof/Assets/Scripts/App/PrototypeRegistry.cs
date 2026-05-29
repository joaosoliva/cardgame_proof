using System.Collections.Generic;
using CardgameProof.Prototypes.ArchiveInvestigation;
using CardgameProof.Prototypes.ForgottenNamesExpedition;
using CardgameProof.Prototypes.ScienceCardGame;

namespace CardgameProof.App
{
    public static class PrototypeRegistry
    {
        private static readonly IReadOnlyList<PrototypeDefinition> prototypes = new List<PrototypeDefinition>
        {
            new PrototypeDefinition(
                PrototypeId.ArchiveInvestigation,
                "Arquivo da Investigação",
                "Protótipo existente de investigação acadêmica em modo pass-and-play.",
                () => new ArchiveInvestigationPrototypeModule()),
            new PrototypeDefinition(
                PrototypeId.ScienceCardGame,
                "Science Card Game",
                "Módulo inicial para a simulação tabletop de conexões entre personagens científicos.",
                () => new ScienceCardGameModule()),
            new PrototypeDefinition(
                PrototypeId.ForgottenNamesExpedition,
                "A Expedição dos Nomes Esquecidos",
                "Um jogo narrativo sobre ciência, memória e nomes que quase esquecemos.",
                () => new ForgottenNamesExpeditionModule())
        };

        public static IReadOnlyList<PrototypeDefinition> All => prototypes;
    }
}
