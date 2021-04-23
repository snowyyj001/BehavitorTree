local node={

{id=1,type=1,parent=0},

{id=2,type=101,parent=1},

{id=3,type=101,parent=2},

{id=4,type=200,parent=3,file="Move",param="2"},

{id=5,type=200,parent=3,file="Stand",param=""},

{id=6,type=200,parent=2,file="AddBuff",param="1001010"},

{id=7,type=100,parent=2},

{id=8,type=200,parent=7,file="Spell",param="200"},

{id=9,type=200,parent=7,file="Speak",param=""},

}

return node
