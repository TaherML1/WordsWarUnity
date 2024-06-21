const functions = require("firebase-functions");
const admin = require("firebase-admin");
admin.initializeApp();

exports.ExtraTimeHint = functions.https.onCall(async (data, context) => {
  const gameId = data.gameId;
  const playerId = data.playerId;

  const gameRef = admin.database().ref(`games/${gameId}`);
  const playerRef = admin.database().ref(`players/${playerId}`);

  try {
    const gameSnapshot = await gameRef.once("value");
    const playerSnapshot = await playerRef.once("value");

    if (!gameSnapshot.exists() || !playerSnapshot.exists()) {
      throw new functions.https.HttpsError("not-found"
          , "Game or Player not found");
    }

    const game = gameSnapshot.val();
    const player = playerSnapshot.val();

    if (player.extraTimeHints > 0) {
      const newTime = game.gameInfo.timer + 10; // Add 10 seconds
      await gameRef.child("gameInfo/timer").set(newTime);
      await playerRef.child("extraTimeHints").set(player.extraTimeHints - 1);

      return {
        success: true, newTime: newTime, remainingHints:
              player.extraTimeHints - 1,
      };
    } else {
      return {success: false, message: "No extra time hints available"};
    }
  } catch (error) {
    console.error("Error using extra time hint: ", error);
    throw new functions.https.HttpsError("internal",
        "Error using extra time hint", error);
  }
});
