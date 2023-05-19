import { mudConfig, resolveTableId } from "@latticexyz/world/register";

export default mudConfig({
  tables: {
    /*
     * TODO:
     * - Position: (x: int32, y: int32),
     * - Health: uint32,
     * - Player: bool,
     * - Damage: uint32
     */
  },
  modules: [
    // TODO: Add reverse lookup for Position
  ],
});
