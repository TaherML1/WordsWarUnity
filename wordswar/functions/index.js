/* eslint-disable max-len */
const functions = require("firebase-functions");
const admin = require("firebase-admin");
admin.initializeApp();

exports.incrementCoinsAD = functions.https.onCall(async (data, context) => {
  const userId = data.userId;
  const coinsToAdd = data.coinsToAdd;

  if (!userId || !coinsToAdd) {
    throw new functions.https.HttpsError("invalid-argument", "The function must be called with userId and coinsToAdd.");
  }
  console.log("coins to add : " + coinsToAdd);

  // Validate coinsToAdd is within the expected range
  if (coinsToAdd < 10 || coinsToAdd > 20) {
    throw new functions.https.HttpsError("invalid-argument", "coinsToAdd must be between 10 and 20.");
  }

  const userDoc = admin.firestore().collection("users").doc(userId);
  const userSnapshot = await userDoc.get();

  if (!userSnapshot.exists) {
    throw new functions.https.HttpsError("not-found", "User document not found.");
  }

  const currentCoins = userSnapshot.data().coins || 0;
  const newCoins = currentCoins + coinsToAdd;

  await userDoc.update({coins: newCoins});

  return {newCoins: newCoins};
});
