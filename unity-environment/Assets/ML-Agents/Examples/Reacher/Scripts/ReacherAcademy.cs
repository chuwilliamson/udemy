using MLAgents;

public class ReacherAcademy : Academy {

    public float goalSize;
    public float goalSpeed;


    public override void AcademyReset()
    {
        goalSize = resetParameters["goal_size"];
        goalSpeed = resetParameters["goal_speed"];
    }

    public override void AcademyStep()
    {


    }

}
