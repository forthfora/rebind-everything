using ImprovedInput;

namespace RebindEverything;

public static class Rebind_Hooks_Saint
{
    public static void ApplyHooks()
    {
        On.Player.ClassMechanicsSaint += Player_ClassMechanicsSaint;
        On.Player.TongueUpdate += Player_TongueUpdate;
    }

    // Ascend, Aim Ascend & Tongue
    private static void Player_ClassMechanicsSaint(On.Player.orig_ClassMechanicsSaint orig, Player self)
    {
        var wasInput0 = self.input[0];
        var wasInput1 = self.input[1];

        var wasWantToJump = self.wantToJump;
        var wasAscension = self.monkAscension;

        if (self.IsAscendCustomInput())
        {
            var ascensionInput = self.JustPressed(Input_Helpers.Ascend);

            self.wantToJump = ascensionInput ? 1 : 0;
            self.input[0].pckp = ascensionInput && !self.monkAscension;
        }

        if (self.IsAimAscendCustomInput())
        {
            var moveAscensionInput = self.AimAscendPressed();

            if (self.monkAscension)
            {
                self.input[0].thrw = moveAscensionInput;
            }
        }

        if (self.IsGrappleCustomInput())
        {
            self.input[0].jmp = self.JustPressed(Input_Helpers.Grapple);

            if (self.JustPressed(Input_Helpers.Grapple))
            {
                self.input[1].jmp = false;
                self.input[0].pckp = false;
            }
        }

        orig(self);

        if (wasAscension == self.monkAscension)
        {
            self.wantToJump = wasWantToJump;
        }

        self.input[0] = wasInput0;
        self.input[1] = wasInput1;
    }

    // Tongue
    private static void Player_TongueUpdate(On.Player.orig_TongueUpdate orig, Player self)
    {
        var wasJmpInput = self.input[0].jmp;
        var wasJmpInputLastFrame = self.input[1].jmp;

        if (self.IsGrappleCustomInput())
        {
            var grappleInput = self.JustPressed(Input_Helpers.Grapple);

            self.input[0].jmp = grappleInput;
            self.input[1].jmp = false;
        }

        orig(self);

        self.input[0].jmp = wasJmpInput;
        self.input[1].jmp = wasJmpInputLastFrame;
    }
}
