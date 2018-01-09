using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace PrimeTesting.enumerable
{
    public interface ISystemService
    {
        DateTime CurrentDateTime { get; }
    }

    public class SystemService : ISystemService
    {
        private static readonly TimeSpan LocalUtcOffset;

        static SystemService()
        {
            LocalUtcOffset = TimeZone.CurrentTimeZone.GetUtcOffset(DateTime.Now);
        }

        public DateTime CurrentDateTime => DateTime.SpecifyKind(DateTime.UtcNow + LocalUtcOffset, DateTimeKind.Local);
    }

    [TestClass]
    public class DateTimeFixture
    {
        private DateTime _current;
        private DateTime _expected;
        private SystemService _systemService;

        [TestInitialize]
        public void BeforeEachTest()
        {
            _systemService = new SystemService();
            _expected = DateTime.Now;
            _current = _systemService.CurrentDateTime;
        }

        [TestMethod]
        public void Should_return_the_correct_day()
        {
            Assert.AreEqual(_current.Day, _expected.Day);
        }

        [TestMethod]
        public void Should_return_the_correct_hour()
        {
            Assert.AreEqual(_current.Hour, _expected.Hour);
        }

        [TestMethod]
        public void Should_return_the_correct_kind()
        {
            Assert.AreEqual(_current.Kind, _expected.Kind);
        }

        [TestMethod]
        public void Should_return_the_correct_millisecond()
        {
            Assert.AreEqual(_current.Millisecond, _expected.Millisecond);
        }

        [TestMethod]
        public void Should_return_the_correct_minute()
        {
            Assert.AreEqual(_current.Minute, _expected.Minute);
        }

        [TestMethod]
        public void Should_return_the_correct_month()
        {
            Assert.AreEqual(_current.Month, _expected.Month);
        }

        [TestMethod]
        public void Should_return_the_correct_second()
        {
            Assert.AreEqual(_current.Second, _expected.Second);
        }

        [TestMethod]
        public void Should_return_the_correct_year()
        {
            Assert.AreEqual(_current.Year, _expected.Year);
        }
    }
}