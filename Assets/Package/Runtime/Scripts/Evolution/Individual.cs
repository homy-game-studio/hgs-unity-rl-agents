namespace HGS.RLAgents.Evolution
{
    public enum EvaluationState
    {
        Pending,
        Running,
        Evaluated
    }

    public struct Individual
    {
        public int Id { get; set; }
        public Genome Genome { get; set; }
        public float Fitness { get; set; }
        public EvaluationState State { get; set; }

        public bool IsEvaluated => State == EvaluationState.Evaluated;
    }
}
