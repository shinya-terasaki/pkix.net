﻿using System;
using System.Linq;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using SysadminsLV.Asn1Parser;
using SysadminsLV.Asn1Parser.Universal;

namespace SysadminsLV.PKI.Cryptography {
    class RsaPublicKey : RawPublicKey {
        static readonly Oid _oid = new Oid(AlgorithmOids.RSA);
        RSA rsa;

        public RsaPublicKey(PublicKey publicKey) : base(_oid) {
            if (publicKey == null) {
                throw new ArgumentNullException(nameof(publicKey));
            }
            if (publicKey.Oid.Value != Oid.Value) {
                throw new ArgumentException("Public key algorithm is not RSA.");
            }
            decodePkcs1Key(publicKey.EncodedKeyValue.RawData);
        }
        public RsaPublicKey(Byte[] rawData, KeyPkcsFormat keyFormat) : base(_oid) {
            if (rawData == null) {
                throw new ArgumentNullException(nameof(rawData));
            }

            switch (keyFormat) {
                case KeyPkcsFormat.Pkcs1:
                    decodePkcs8Key(rawData);
                    break;
                case KeyPkcsFormat.Pkcs8:
                    decodePkcs8Key(rawData);
                    break;
                default: throw new ArgumentOutOfRangeException();
            }
        }

        public Byte[] Modulus { get; private set; }
        public Byte[] PublicExponent { get; private set; }

        void decodePkcs1Key(Byte[] rawPublicKey) {
            var asn = new Asn1Reader(rawPublicKey);
            asn.MoveNextAndExpectTags((Byte)Asn1Type.INTEGER);
            Byte[] bytes = asn.GetPayload();

            Modulus = bytes[0] == 0
                ? bytes.Skip(1).ToArray()
                : bytes;
            asn.MoveNextAndExpectTags((Byte)Asn1Type.INTEGER);
            PublicExponent = asn.GetPayload();
        }
        void decodePkcs8Key(Byte[] rawData) {
            var asn = new Asn1Reader(rawData);
            asn.MoveNextAndExpectTags(0x30);
            Int32 offset = asn.Offset;
            asn.MoveNextAndExpectTags((Byte)Asn1Type.OBJECT_IDENTIFIER);
            Oid oid = ((Asn1ObjectIdentifier)asn.GetTagObject()).Value;
            if (oid.Value != Oid.Value) {
                throw new ArgumentException("Public key algorithm is not RSA.");
            }
            asn.MoveToPoisition(offset);
            asn.MoveNextCurrentLevelAndExpectTags((Byte)Asn1Type.BIT_STRING);
            asn.MoveNextAndExpectTags(0x30);
            decodePkcs1Key(asn.GetTagRawData());
        }

        public override AsymmetricAlgorithm GetAsymmetricKey() {
            if (rsa != null) {
                return rsa;
            }
            var rsaParams = new RSAParameters {
                Modulus = Modulus,
                Exponent = PublicExponent
            };
            rsa = RSA.Create();
            rsa.ImportParameters(rsaParams);
            return rsa;
        }

        /// <inheritdoc />
        public override void Dispose() {
            rsa?.Dispose();
        }
    }
}
