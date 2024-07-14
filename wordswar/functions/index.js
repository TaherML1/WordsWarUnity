/* eslint-disable max-len */
/* eslint-disable require-jsdoc */
const functions = require("firebase-functions");
const admin = require("firebase-admin");
admin.initializeApp();

exports.setTimer = functions.https.onRequest(async (req, res) => {
  try {
    // Verify authentication
    const user = await validateFirebaseIdToken(req);

    // Set the timer for the user
    const duration = 3600000; // 1 hour in milliseconds
    const endTime = admin.firestore.Timestamp.fromMillis(Date.now() + duration);

    await admin.firestore().collection("users").doc(user.uid).set({
      endTime: endTime,
    });

    res.status(200).send("Timer set successfully");
  } catch (error) {
    console.error("Error setting timer:", error);
    res.status(500).send("Error setting timer: " + error.message);
  }
});

// Function to validate Firebase ID token
async function validateFirebaseIdToken(req) {
  if (!req.headers.authorization || !req.headers.authorization.startsWith("Bearer ")) {
    throw new Error("Unauthorized");
  }

  const idToken = req.headers.authorization.split("Bearer ")[1];
  try {
    const decodedToken = await admin.auth().verifyIdToken(idToken);
    return decodedToken;
  } catch (error) {
    throw new Error("Unauthorized");
  }
}
