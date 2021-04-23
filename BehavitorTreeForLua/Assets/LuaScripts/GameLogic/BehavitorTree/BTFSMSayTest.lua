local BTFsm = require "GameLogic.BehavitorTree.Base.BTFsm"
local BTFSMSayTest = BaseClass("BTFSMIdleTest", BTFsm)

BTFSMSayTest.ctor = function(self, btnode)
    BTFSMSayTest.super.ctor(self, btnode)
end

BTFSMSayTest.OnEnter = function(self, owner)
    print("BTFSMSayTest.OnEnter")
end


BTFSMSayTest.OnExec = function(self, owner)
    print(self.btnode.actionParam)
    return BehavitorTree.Result.Successs
end

BTFSMSayTest.OnExit = function(self, owner)
    print("BTFSMSayTest.OnExit")
end

return BTFSMSayTest