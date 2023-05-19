import { mudConfig, resolveTableId } from "@latticexyz/world/register";

export default mudConfig({
  tables: {
    Position: {
      schema: {
        x: "int32",
        y: "int32",
      },
    },
    Health: {
      schema: {
        value: "uint32",
      },
    },
    Player: "bool",
    Damage: "uint32",
  },
  modules: [
    {
      name: "KeysWithValueModule",
      root: true,
      args: [resolveTableId("Position")],
    },
  ],
});
