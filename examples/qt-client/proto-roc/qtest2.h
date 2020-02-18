#ifndef GENPROTO_TEST2_H
#define GENPROTO_TEST2_H
#include <QObject>
namespace Tests {
class QTest2 : public QObject {
	Q_OBJECT
public:
	QTest2(QObject* parent = nullptr);
	~QTest2();
};
}
#endif // GENPROTO_TEST2_H
