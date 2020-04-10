namespace EarnYourStripes
{
    [KSPScenario(ScenarioCreationOptions.AddToAllGames, GameScenes.SPACECENTER, GameScenes.FLIGHT, GameScenes.TRACKSTATION, GameScenes.EDITOR)]
    internal class StripesData : ScenarioModule
    {
        public override void OnSave(ConfigNode node)
        {
            node.AddValue("firstRun", EarnYourStripes.Instance.firstRun);
            EarnYourStripes.Instance.OnSave(node);
        }
        public override void OnLoad(ConfigNode node)
        {
            node.TryGetValue("firstRun", ref EarnYourStripes.Instance.firstRun);
            EarnYourStripes.Instance.OnLoad(node);
        }
    }
}
