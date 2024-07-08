/* eslint-disable no-undef */
/* eslint-disable max-len */
const {onCall} = require("firebase-functions/v2/https");
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
  for (let j = 0; j < 6; j++) {
    roomId += possibleChars.charAt(Math.floor(Math.random() *
            possibleChars.length));
  }
  return roomId;
}

// Create Special Room Cloud Function
exports.createSpecialRoom2 = onCall(async (request) => {
  console.log("createSpecialRoom function triggered.");

  const {auth} = request;
  // Check if the user is authenticated
  if (!auth) {
    console.error("Unauthenticated request.");
    throw new functions.https.HttpsError("unauthenticated", "Request not authenticated.");
  }

  const playerId = auth.uid;
  console.log(`Authenticated user: ${playerId}`);

  // Generate a unique room ID
  const roomId = generateRoomId();
  console.log(`Generated room ID: ${roomId}`);

  // Initial room data
  const roomData = {
    roomId: roomId,
    host: playerId,
    players: [playerId], // Host is the first player
    status: "waiting", // Room is waiting for players
  };
  console.log("Initial room data:", roomData);

  try {
    // Save the room data under the 'specialrooms' node in the database
    await database.ref("specialrooms/" + roomId).set(roomData);
    console.log(`Room data saved to database under 'specialrooms/${roomId}'.`);

    console.log("room id is : " + roomId);
    // Return the room ID and a success message
    const response = {roomId: roomId, message: "Room created successfully!"};
    console.log("Function response:", response);
    return response;
  } catch (error) {
    console.error("Error saving room data to the database:", error);
    throw new functions.https.HttpsError("unknown", "Failed to create room.", error);
  }
});
