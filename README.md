# Black Book

**Black Book** is a portable, profile-based, personal correspondence manager. It is a very native WPF application written in C# for .NET 8+  

## Security Model

Because *Black Book* is intended to perform the role of the proverbial *"little black book"* the quintessential doctrine of practice here is *security* or, if you think the tone is a bit harsh, *privacy*  

- **Password Protection:** A natively WPF portable program where a profile is only protected by a password? Suspectedly susceptible-sounding situation, surely. The threat to password protection from someone who has gained filesystem access / obtained a local copy of *[a profile]*, has been addressed with the addition of **Argon2id**; which is configured to make most types of shitty dictionary attack be *as time consuming, expensive, futile, and frustrating as possible* ... although you can't currently configure the intensity of argon2id per profile, the parameters that exist already make profiles with even the shittest passwords rock solid and impenetrable. See how far you get with *"`file`"* ...

- **AEAD Encryption:** *Black Book* uses AEAD encryption ... somewhere ... involving **ChaCha20-Poly1305** which is both quick and secure at the same time. All the cryptographic shit constructs a 256-bit key that is derived from *[not even your original]* password / argon2id / other things ... and includes an authenticity tag, so *things like **tampering** are detected immediately*, once `fucking about` of any kind has been discovered, any functions *heuristically found to decrypt data either as one step in a sequence of functions, or as a result of the sequence itself* will no longer be operational. At even the mere hint of tampering, the profile is bricked instantly thus making it completely useless to *everyone* ... once this happens it won't be difficult to work out who was at your desk / nearby at the time it happened

- **Secure Profile Managment:** A profile on *Black Book* is made up of *things*. A *thing* can be: `person`, `company`, `interaction`, `situation`, or `objective`. When *things* are fetched from the saved profile data, they exist only in memory. *Doing* something - *[addition, changes, amendments, removal, updates]* takes place in memory, is [given a new / attached to an existing] unique ID, then the entire profile structure is completely encrypted; only when this has been done is it saved to disk. Nothing *not even temporarily* is saved to disk. When a profile is exited / the application is closed, ***everything that was in memory: the secret composite things comprising the constructed key that allows access and changes to the profile, the entire structure and contents of "`file`", everything else*** is **completely wiped from memory** - so even if someone kicks the door down and you have only a very quick moment with the chance to make only a very brief movement - simply using *alt+F4* is all that you need, and everything in your *black book* is completely protected from anyone who sits at your desk or tries to access your hardware for the purposes of digital forensic analysis

- **The File File:** If you bother to look at the structure of how a profile is stored in the filesystem, you may come across a second file ... *`file.file`*. Although this will differ from profile to profile, this will always be a teeny tiny file - says absolutely fuck all about where it belongs, conforms to no protocol or standards specification, *isn't concluded by quantum* - let's call it 2KB, and then think about how it will fit on something like a *microSD card* which takes up no space in the physical world at all. Now we have considered those formidable facts, here's the absolute kicker ... ***without `file.file` the profile is completely inaccessible***. To fashion a functionally prominent use case, the understated *second file* in the profile name's directory can be removed completely, and it doesn't matter what tactics or hardware is used on `file`, because it's completely impossible to access or alter until `file.file` is replaced. This gives you the opportunity to secure any profile, in a way that surpasses computation / mathematics, simply by removing the other file and keeping it somewhere else

- **Forensic Delete:** *Black Book* provides you with **forensic grade profile deletion**. Not only does it completely rinse the fuck out of `file` 58 times, such that even the original size of the data on the filesystem can't even determined; it leaves a forensic analyst behind something special. By the time they realise what they're dealing with ... if they ever actually reach that conclusion ... they will be trying real hard to avoid giving you the satisfaction, competence in their very profession will not go down well - especially if you've got them to waste thousands of pounds in overtime and expenses / outsourcing / trying to find something significant in such a relentlessly promising false-hope of a dead end ... before they have nothing to charge you with and have to let you go

## Philosophy – Who Is This For?

*Black Book* intends to adopt the characteristics of the little black book in a digitally prolific age where privacy is at its most premium. You might get some personal space in your workplace copy of *outlook* but in real life you need somewhere to keep your innermost workings

## License

**Black Book is free for non-commercial use.** You are permitted to use the application (and view/modify its source code) for personal or organizational purposes at no charge. If you adapt or redistribute the software in any form, **attribution is required** – please credit **westid / INCORRIGO SYX DIGITAL COMMUNICATION SYSTEMS** and include a reference to [incorrigo.io](https://incorrigo.io/). 

For **commercial use** (if you or your organization derive financial gain from using or distributing Black Book or a derivative), you **must obtain a commercial license**. This typically involves arranging a royalty or licensing agreement with the author. *(See the full LICENCE.md for details, and contact **operations@incorrigo.io** for commercial licensing inquiries.)*

Third party compliance and acknowledgement(s): This entire project is a *completely native WPF .NET 8.0 C# Application* except for the following classes which were used at certain points in the source code:  

*Black Book* uses **Konscious.Security.Cryptography** which is covered in the third party acknowledgements *[3PL]* directory by *L01.txt*

---
