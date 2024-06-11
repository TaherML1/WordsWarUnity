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

// Create Special Room Cloud Function
exports.createSpecialRoom = functions.https.onCall(async (data, context) => {
  if (!context.auth) {
    throw new functions.https.HttpsError("unauthenticated", "noauthenticated.");
  }

  const playerId = context.auth.uid;

  // Generate a unique room ID
  const roomId = generateRoomId();

  // Initial room data
  const roomData = {
    roomId: roomId,
    host: playerId,
    players: [playerId], // Host is the first player
    status: "waiting", // Room is waiting for players
  };

  try {
    // Save the room data under the 'specialrooms' node in the database
    await database.ref("specialrooms/" + roomId).set(roomData);

    return {roomId: roomId, message: "Special room created successfully!"};
  } catch (error) {
    throw new functions.https.HttpsError("unknown", "Failed to  room.", error);
  }
});

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
 * Initializes a game with the provided room ID and player list under
 * @param {string} roomId - The ID of the room.
 * @param {Array<string>} players - The list of player IDs.
 */
async function initializeSpecialGame(roomId, players) {
  const gameId = generateRoomId(); // Using the same ID generation function

  const gameData = {
    gameInfo: {
      gameId: gameId,
      playersIds: players,
      scores: players.reduce((acc, playerId) => ({...acc, [playerId]: 0}), {}),
      usedWords: players.reduce((acc, playerId) => ({
        ...acc, [playerId]:
                  [""],
      }), {}),
      timer: 15,
    },
    turn: players[0], // Starting the turn with the first player
  };

  await database.ref("games/" + gameId).set(gameData);
}
