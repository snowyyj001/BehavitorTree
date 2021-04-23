local BTFsm = require "GameLogic.BehavitorTree.Base.BTFsm"
local BTFSMIdleTest = BaseClass("BTFSMIdleTest", BTFsm)

BTFSMIdleTest.ctor = function(self, btnode)
    BTFSMIdleTest.super.ctor(self, btnode)
end

BTFSMIdleTest.OnEnter = function(self, owner)
    print("BTFSMIdleTest.OnEnter")
    owner.m_Animator:Play ("stay")
    self.staytime = 5
end


BTFSMIdleTest.OnExec = function(self, owner)
    self.staytime = self.staytime - 1
    if self.staytime > 0 then
        return BehavitorTree.Result.Running
    else
        return BehavitorTree.Result.Successs
    end
end

BTFSMIdleTest.OnExit = function(self, owner)
    print("BTFSMIdleTest.OnExit")
end

return BTFSMIdleTest