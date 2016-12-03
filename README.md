# Bitforum
A C# blockchain proof-of-concept for Usenet-style discourse

## Protocol

Bitforum is a block-chain based ecosystem for distributing messages to public forums in a style similar to Usenet.  The Bitforum protocol is inspired heavily by Bitcoin, with some lessons learned and influence drawn from Freenet and Bitmessage as well.  The goals of the protcool are to:

1. Provide a means by which users can post messages to forums or newsgroups.
2. Protect the anonymity of users
  * Users are not required to identify themselves
  * Users are not required to authenticate to the network to retrieve or send messages
3. Protect the integrity of messages
  * Messages are signed using blockchain techniques to protect message integrity
  * The protocol does not support message deletion or cancellation to prevent centralized control or censorship
4. Clients must be participating peers and not leech resources from the network
  * To post a message, clients provide a **messaging** proof-of-work hash
    * Normal clients with a full copy of the Bitforum blockchain provide a messaging proof-of-work hash that proves they have a copy of some portion of the blockchain
    * Lightweight (mobile, embedded) clients may not have a full copy of the Bitforum blockchain, but they can provide a messaging proof-of-work hash that provides they have distributed a portion of the blockchain to others in the past.  (Credit can be granted, accumulated, and spent for propogating information)
  * To retrieve portions of the blockchain, clients must have to answer challenges to prove they are willing to put in work to consume resources from other nodes to retrieve data.
5. The protocol is not intended as a store of value
6. The protocol is not intended as a closed messaging system among small groups; however, other forms of encryption can be layered on to of Bitforum.
