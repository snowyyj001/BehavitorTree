local BTNode = require "GameLogic.BehavitorTree.Base.BehavitorTree"

local BTManager = BaseClass("BTManager")

BTManager.ctor = function(self, owner)
    self.treeList = {}
    self.curBtree = nil
    self.curBtreeIndex = nil
    self.owner = owner
end

BTManager.Parse = function(self, btreetxt)
    local node = BehavitorTree.CFG[btreetxt]
    local root = nil
    local nodeids = {}
    for i = 1, #node do
        local btnode = BTNode.New()
        btnode.nodeType = node[i].type
        btnode.actionFunc = node[i].file
        btnode.actionParam = node[i].param
        if i == 1 then
            root = btnode
        end
        nodeids[node[i].id] = btnode
        if i > 1 then
            table.insert(nodeids[node[i].parent].childList, btnode)
        end
    end
    return root
end


BTManager.AddBtree = function(self, btreetxt)
    local bt = self:Parse(btreetxt)
    table.insert(self.treeList, bt)
end

BTManager.Start = function(self, index)
    if next(self.treeList) == nil then
        return
    end
    self.curBtreeIndex = index
    self.curBtree = self.treeList[self.curBtreeIndex]
end

BTManager.Update = function(self, dt)
    if self.curBtree == nil then
        return
    end
    local result = self.curBtree:Update(self.owner, dt)
    if result ~= BehavitorTree.Result.Running then
        self:Start(self.curBtreeIndex + 1)
    end
end

return BTManager