// SPDX-License-Identifier: MIT
pragma solidity >=0.8.0;
import { System } from "@latticexyz/world/src/System.sol";
import { getKeysWithValue } from "@latticexyz/world/src/modules/keyswithvalue/getKeysWithValue.sol";
// TODO: import tables
import { addressToEntityKey } from "../addressToEntityKey.sol";

contract AttackSystem is System {
  function attack(int32 x, int32 y) public {
    bytes32 player = addressToEntityKey(address(_msgSender()));

    // TODO: get all coords surrounding the target (including the target)
    // PositionData[] memory neighbors = mooreNeighborhood(PositionData(x, y)); 

    // TODO: iterate over all coords surrounding the target
    // for (uint i = 0; i < neighbors.length; i++) {
    //   PositionData memory neighbor = neighbors[i];
    //   bytes32[] memory atPosition = getKeysWithValue(PositionTableId, Position.encode(neighbor.x, neighbor.y));
    //   if (atPosition.length == 1) {
    //     attackTarget(player, atPosition);
    //    }
    // }
  }

	// TODO
  // function attackTarget(bytes32 player, bytes32[] memory atPosition) internal {
  //   bytes32 defender = atPosition[0];

  //   require(Player.get(defender), "target is not a player");
  //   require(Health.get(defender) > 0, "target is dead");

  //   uint32 playerDamage = Damage.get(player);
  //   uint32 defenderHealth = Health.get(defender);
  //   uint32 newHealth = defenderHealth - playerDamage;
  //   if (newHealth <= 0) {
  //     Health.deleteRecord(defender);
  //     Position.deleteRecord(defender);
  //     Player.deleteRecord(defender);
  //   } else {
  //     Health.set(defender, newHealth);
  //   } 
  // }

// TODO: Uncomment once you have the Position Table set up
// function mooreNeighborhood(PositionData memory center) internal pure returns (PositionData[] memory) {
//     PositionData[] memory neighbors = new PositionData[](9);
//     uint256 index = 0;

//     for (int32 x = -1; x <= 1; x++) {
//         for (int32 y = -1; y <= 1; y++) {
//             neighbors[index] = PositionData(center.x + x, center.y + y);
//             index++;
//         }
//     }

//     return neighbors;
// }
}
