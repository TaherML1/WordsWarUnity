/* eslint-disable max-len */
const functions = require("firebase-functions");
const admin = require("firebase-admin");
admin.initializeApp();

exports.increaserefreshedTickets = functions.https.onCall(async (data, context) => {
  const userId = context.auth.uid;

  if (!userId) {
    throw new functions.https.HttpsError("unauthenticated", "User is not authenticated.");
  }

  const userRef = admin.firestore().collection("users").doc(userId).collection("hints").doc("hintsData");
  const userDoc = await userRef.get();

  if (!userDoc.exists) {
    throw new functions.https.HttpsError("not-found", "User data not found.");
  }

  const currentTickets = userDoc.data().refreshedTickets;

  if (currentTickets < 3) {
    await userRef.update({
      refreshedTickets: admin.firestore.FieldValue.increment(1),
    });
    return {message: "Tickets increased successfully."};
  } else {
    throw new functions.https.HttpsError("failed-precondition", "Ticket limit reached.");
  }
});
