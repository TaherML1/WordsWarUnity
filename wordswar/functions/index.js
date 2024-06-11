const functions = require("firebase-functions");
const admin = require("firebase-admin");
admin.initializeApp();
const database = admin.database();

/**
 * Generates a random room ID.
 * @return {string} The randomly generated room ID.
 */
function generateRoomId() {
  const possibleChars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnxyz0123456789";
  let roomId = "";
  for (let j = 0; j < 20; j++) {
    roomId += possibleChars.charAt(Math.floor(Math.random() *
          possibleChars.length));
  }
  return roomId;
}


// Join Special Room Cloud Function
exports.joinSpecialRoom = functions.https.onCall(async (data, context) => {
  if (!context.auth) {
    throw new functions.https.HttpsError("unauthenticated", "noauthenticated.");
  }

  const playerId = context.auth.uid;
  const roomId = data.roomId;

  try {
    // Check if the special room exists and is waiting for players
    const roomRef = database.ref("specialrooms/" + roomId);
    const roomSnapshot = await roomRef.once("value");

    if (!roomSnapshot.exists()) {
      throw new functions.https.HttpsError("not-found", " room not found.");
    }

    const roomData = roomSnapshot.val();
    if (roomData.status !== "waiting") {
      throw new functions.https.HttpsError("precondition", "not players.");
    }

    // Add the player to the room
    roomData.players.push(playerId);

    // Update the room status if it's full (assuming 2 players per room for thi
    if (roomData.players.length === 2) {
      roomData.status = "full";
      // Initialize the game if needed
      await initializeSpecialGame(roomId, roomData.players);
    }

    await roomRef.update(roomData);

    return {roomId: roomId, message: "Joined special room successfully!"};
  } catch (error) {
    throw new functions.https.HttpsError("unknown", "Failed to  room.", error);
  }
});

/**
 * Initializes a game with the provided room ID and player list
 * @param {string} roomId - The ID of the room.
 * @param {Array<string>} players - The list of player IDs.
 */
async function initializeSpecialGame(roomId, players) {
  const gameId = generateRoomId(); // Generate a unique game ID

  // Ensure the players array has exactly 2 players
  if (players.length !== 2) {
    throw new Error("Game initialization requires exactly 2 players.");
  }

  // Construct the game data similar to your provided structure
  const gameData = {
    gameInfo: {
      gameId: gameId, // Unique game ID
      playersIds: players, // List of player IDs

      // Initialize scores for each player to 0
      scores: {
        [players[0]]: 0,
        [players[1]]: 0,
      },

      // Initialize used words lists for each player
      usedwords: {
        [players[0]]: [""],
        [players[1]]: [""],
      },

      timer: 15, // Set the initial timer value to 15 seconds
    },
    turn: players[0], // Set the first player's turn
  };

  // Save the game data to the 'games' node in the Firebase Realtime Database
  await database.ref("games/" + gameId).set(gameData);
}
